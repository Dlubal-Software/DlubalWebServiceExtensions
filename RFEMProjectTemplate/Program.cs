using System;
using System.IO;
using Dlubal.WS.Rfem6.Application;
using ApplicationClient = Dlubal.WS.Rfem6.Application.RfemApplicationClient;
using Dlubal.WS.Rfem6.Model;
using ModelClient = Dlubal.WS.Rfem6.Model.RfemModelClient;
using System.Net.Http;
using System.ServiceModel;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RFEMProjectTemplate
{




    class Program
    {
        public static EndpointAddress Address { get; set; } = new EndpointAddress("http://localhost:8081");


        private static BasicHttpBinding Binding
        {
            get
            {
                BasicHttpBinding binding = new BasicHttpBinding
                {
                    // Send timeout is set to 180 seconds.
                    SendTimeout = new TimeSpan(0, 0, 180),
                    UseDefaultWebProxy = true,
                    MaxReceivedMessageSize = 1000000000,
                };

                return binding;
            }
        }


        static string DecodeHtmlString(string myEncodedString)
        {
            StringWriter myWriter = new StringWriter();
            // Decode the encoded string.
            System.Net.WebUtility.HtmlDecode(myEncodedString, myWriter);
            string myDecodedString = myWriter.ToString();

            return myDecodedString;
        }


        //private static RfemApplicationClient application = null;
        private static ApplicationClient application = null;

        static void Main(string[] args)
        {

            string CurrentDirectory = Directory.GetCurrentDirectory();
            #region Application Settings
            try
            {
                application_information ApplicationInfo;
                try
                {
                    // connects to RFEM6 or RSTAB9 application
                    application = new ApplicationClient(Binding, Address);

                }
                catch (Exception exception)
                {
                    if (application != null)
                    {
                        if (application.State != CommunicationState.Faulted)
                        {
                            application.Close();
                        }
                        else
                        {
                            application.Abort();
                        }

                        application = null;
                    }
                }
                finally
                {
                    ApplicationInfo = application.get_information();
                    Console.WriteLine("Name: {0}, Version:{1}, Type: {2}, language: {3} ", ApplicationInfo.name, ApplicationInfo.version, ApplicationInfo.type, ApplicationInfo.language_name);
                }
                #endregion

                // creates new model
                string modelName = "MyTestModel";
                string modelUrl = application.new_model(modelName);


                #region new model
                // connects to RFEM6/RSTAB9 model
                ModelClient model = new ModelClient(Binding, new EndpointAddress(modelUrl));
                model.reset();
                #endregion


                #region AddonList
                addon_list_type addon = model.get_addon_statuses();
                Console.WriteLine("Material nonlinear analysis active?: {0}", addon.analysis.material_nonlinear_analysis_active ? "Yes" : "No");
                Console.WriteLine("Structure stability active?: {0}", addon.analysis.structure_stability_active ? "Yes" : "No");
                Console.WriteLine("Construction stages active?: {0}", addon.analysis.construction_stages_active ? "Yes" : "No");
                Console.WriteLine("Time dependent active?: {0}", addon.analysis.time_dependent_active ? "Yes" : "No");
                Console.WriteLine("Form finding active?: {0}", addon.analysis.form_finding_active ? "Yes" : "No");
                Console.WriteLine("Warping active?: {0}", addon.analysis.torsional_warping_active ? "Yes" : "No");
                Console.WriteLine("Modal analysis active?: {0}", addon.dynamic_analysis_settings.modal_active ? "Yes" : "No");
                Console.WriteLine("Spectral analysis active?: {0}", addon.dynamic_analysis_settings.spectral_active ? "Yes" : "No");
                Console.WriteLine("Building model active?: {0}", addon.special_solutions.building_model_active ? "Yes" : "No");
                Console.WriteLine("Wind simulation active?: {0}", addon.special_solutions.wind_simulation_active ? "Yes" : "No");
                Console.WriteLine("Geo-technical analysis active?: {0}", addon.special_solutions.geotechnical_analysis_active ? "Yes" : "No");
                Console.WriteLine("Stress analysis active?: {0}", addon.design_addons.stress_analysis_active ? "Yes" : "No");
                Console.WriteLine("Concrete design active?: {0}", addon.design_addons.concrete_design_active ? "Yes" : "No");
                Console.WriteLine("Steel design active?: {0}", addon.design_addons.steel_design_active ? "Yes" : "No");
                Console.WriteLine("Timber design active?: {0}", addon.design_addons.timber_design_active ? "Yes" : "No");
                Console.WriteLine("Masonry design active?: {0}", addon.masonry_design_active ? "Yes" : "No");
                Console.WriteLine("Aluminum design active?: {0}", addon.design_addons.aluminum_design_active ? "Yes" : "No");
                Console.WriteLine("Steel joints design active?: {0}", addon.design_addons.steel_joints_active ? "Yes" : "No");
                Console.WriteLine("Cost estimation active?: {0}", addon.analysis.cost_estimation_active ? "Yes" : "No");

                addon.design_addons.concrete_design_active = true;
                addon.design_addons.concrete_design_activeSpecified = true;
                try
                {
                    model.begin_modification("Set AddOns");
                    model.set_addon_statuses(addon);
                }
                catch (Exception exception)
                {
                    model.cancel_modification();
                    throw;
                }
                finally
                {
                    try
                    {
                        model.finish_modification();
                    }
                    catch (Exception exception)
                    {
                        throw;
                    }
                }
                #endregion


                material materialConcrete = new material
                {
                    no = 1,
                    name = "C20/25 | EN 1992-1-1:2004/A1:2014"
                };

                material materialUser = new material
                {
                    no = 6,
                    name = "UserMaterial",
                    user_defined_name_enabled = true,
                    user_defined_name_enabledSpecified = true,
                    user_defined = true,
                    user_definedSpecified = true,
                    material_type = material_material_type.TYPE_BASIC,
                    material_typeSpecified = true,
                    material_model = material_material_model.MODEL_ISOTROPIC_LINEAR_ELASTIC,
                    material_modelSpecified = true,
                    definition_type = material_definition_type.DERIVED_G,
                    definition_typeSpecified = true,
                    is_temperature_dependent = false,
                    is_temperature_dependentSpecified = true,
                    temperature = new material_temperature_row[]{
                             new material_temperature_row(){
                                 no = 1,
                                 row = new material_temperature(){
                                 elasticity_modulus_global = 2.7e+10,
                                 elasticity_modulus_globalSpecified = true,
                                 elasticity_modulus_x = 2.7e+10,
                                 elasticity_modulus_xSpecified = true,
                                 elasticity_modulus_y = 2.7e+10,
                                 elasticity_modulus_ySpecified = true,
                                 elasticity_modulus_z = 2.7e+10,
                                 elasticity_modulus_zSpecified = true,
                                 shear_modulus_global = 1.125e+10,
                                 shear_modulus_globalSpecified = true,
                                 shear_modulus_yz = 1.125e+10,
                                 shear_modulus_yzSpecified = true,
                                 shear_modulus_xz = 1.125e+10,
                                 shear_modulus_xzSpecified = true,
                                 shear_modulus_xy = 1.125e+10,
                                 shear_modulus_xySpecified =  true,
                                 poisson_ratio_global = 0.2,
                                 poisson_ratio_globalSpecified = true,
                                 poisson_ratio_yz = 0.2,
                                 poisson_ratio_yzSpecified = true,
                                 poisson_ratio_xz = 0.2,
                                 poisson_ratio_xzSpecified = true,
                                 poisson_ratio_xy = 0.2,
                                 poisson_ratio_xySpecified = true,
                                 poisson_ratio_zy = 0.2,
                                 poisson_ratio_zySpecified = true,
                                 poisson_ratio_zx = 0.2,
                                 poisson_ratio_zxSpecified = true,
                                 poisson_ratio_yx = 0.2,
                                 poisson_ratio_yxSpecified = true,
                                 mass_density = 2500,
                                 mass_densitySpecified = true,
                                 specific_weight = 25000,
                                 specific_weightSpecified = true,
                                 thermal_expansion_coefficient_global = 1e-05,
                                 thermal_expansion_coefficient_globalSpecified = true,
                                 thermal_expansion_coefficient_x = 1e-05,
                                 thermal_expansion_coefficient_xSpecified = true,
                                 thermal_expansion_coefficient_y = 1e-05,
                                 thermal_expansion_coefficient_ySpecified = true,
                                 thermal_expansion_coefficient_z = 1e-05,
                                 thermal_expansion_coefficient_zSpecified = true,
                                 division_multiplication_factor = 1,
                                 division_multiplication_factorSpecified = true,
                                 strain_hardening_modulus = 270000,
                                 strain_hardening_modulusSpecified = true,
                                 }

                             }
                         },
                    stiffness_matrix_editable = false,
                    stiffness_matrix_editableSpecified = true,
                    stiffness_modification = false,
                    stiffness_modificationSpecified = true,
                    stiffness_modification_type = material_stiffness_modification_type.STIFFNESS_MODIFICATION_TYPE_DIVISION,
                    stiffness_modification_typeSpecified = true,
                };


                material materialUserThermal = new material
                {
                    no = 7,
                    name = "UserMaterialThermal",
                    user_defined_name_enabled = true,
                    user_defined_name_enabledSpecified = true,
                    user_defined = true,
                    user_definedSpecified = true,
                    material_type = material_material_type.TYPE_BASIC,
                    material_typeSpecified = true,
                    material_model = material_material_model.MODEL_ISOTROPIC_LINEAR_ELASTIC,
                    material_modelSpecified = true,
                    definition_type = material_definition_type.DERIVED_G,
                    definition_typeSpecified = true,
                    is_temperature_dependent = true,
                    is_temperature_dependentSpecified = true,
                    reference_temperature = 293.15,
                    reference_temperatureSpecified = true,
                    temperature_properties_sorted = true,
                    temperature_properties_sortedSpecified = true,
                    temperature = new material_temperature_row[]{
                             new material_temperature_row(){
                                 no = 1,
                                 row = new material_temperature(){
                                 temperature = 293.15,
                                 temperatureSpecified = true,
                                 elasticity_modulus_global = 2.7e+10,
                                 elasticity_modulus_globalSpecified = true,
                                 elasticity_modulus_x = 2.7e+10,
                                 elasticity_modulus_xSpecified = true,
                                 elasticity_modulus_y = 2.7e+10,
                                 elasticity_modulus_ySpecified = true,
                                 elasticity_modulus_z = 2.7e+10,
                                 elasticity_modulus_zSpecified = true,
                                 shear_modulus_global = 1.125e+10,
                                 shear_modulus_globalSpecified = true,
                                 shear_modulus_yz = 1.125e+10,
                                 shear_modulus_yzSpecified = true,
                                 shear_modulus_xz = 1.125e+10,
                                 shear_modulus_xzSpecified = true,
                                 shear_modulus_xy = 1.125e+10,
                                 shear_modulus_xySpecified =  true,
                                 poisson_ratio_global = 0.2,
                                 poisson_ratio_globalSpecified = true,
                                 poisson_ratio_yz = 0.2,
                                 poisson_ratio_yzSpecified = true,
                                 poisson_ratio_xz = 0.2,
                                 poisson_ratio_xzSpecified = true,
                                 poisson_ratio_xy = 0.2,
                                 poisson_ratio_xySpecified = true,
                                 poisson_ratio_zy = 0.2,
                                 poisson_ratio_zySpecified = true,
                                 poisson_ratio_zx = 0.2,
                                 poisson_ratio_zxSpecified = true,
                                 poisson_ratio_yx = 0.2,
                                 poisson_ratio_yxSpecified = true,
                                 mass_density = 2500,
                                 mass_densitySpecified = true,
                                 specific_weight = 25000,
                                 specific_weightSpecified = true,
                                 thermal_expansion_coefficient_global = 1e-05,
                                 thermal_expansion_coefficient_globalSpecified = true,
                                 thermal_expansion_coefficient_x = 1e-05,
                                 thermal_expansion_coefficient_xSpecified = true,
                                 thermal_expansion_coefficient_y = 1e-05,
                                 thermal_expansion_coefficient_ySpecified = true,
                                 thermal_expansion_coefficient_z = 1e-05,
                                 thermal_expansion_coefficient_zSpecified = true,
                                 division_multiplication_factor = 1,
                                 division_multiplication_factorSpecified = true,
                                 strain_hardening_modulus = 270000,
                                 strain_hardening_modulusSpecified = true,
                                 }

                             },
                              new material_temperature_row(){
                                 no = 2,
                                 row = new material_temperature(){
                                 temperature = 303.15,
                                 temperatureSpecified = true,
                                 elasticity_modulus_global =2.8e+10,
                                 elasticity_modulus_globalSpecified = true,
                                 elasticity_modulus_x = 2.8e+10,
                                 elasticity_modulus_xSpecified = true,
                                 elasticity_modulus_y = 2.8e+10,
                                 elasticity_modulus_ySpecified = true,
                                 elasticity_modulus_z =2.8e+10,
                                 elasticity_modulus_zSpecified = true,
                                 shear_modulus_global =11666666666.666668,
                                 shear_modulus_globalSpecified = true,
                                 shear_modulus_yz =11666666666.666668,
                                 shear_modulus_yzSpecified = true,
                                 shear_modulus_xz = 11666666666.666668,
                                 shear_modulus_xzSpecified = true,
                                 shear_modulus_xy = 11666666666.666668,
                                 shear_modulus_xySpecified =  true,
                                 poisson_ratio_global = 0.2,
                                 poisson_ratio_globalSpecified = true,
                                 poisson_ratio_yz = 0.2,
                                 poisson_ratio_yzSpecified = true,
                                 poisson_ratio_xz = 0.2,
                                 poisson_ratio_xzSpecified = true,
                                 poisson_ratio_xy = 0.2,
                                 poisson_ratio_xySpecified = true,
                                 poisson_ratio_zy = 0.2,
                                 poisson_ratio_zySpecified = true,
                                 poisson_ratio_zx = 0.2,
                                 poisson_ratio_zxSpecified = true,
                                 poisson_ratio_yx = 0.2,
                                 poisson_ratio_yxSpecified = true,
                                 mass_density = 2500,
                                 mass_densitySpecified = true,
                                 specific_weight = 25000,
                                 specific_weightSpecified = true,
                                 thermal_expansion_coefficient_global = 1e-05,
                                 thermal_expansion_coefficient_globalSpecified = true,
                                 thermal_expansion_coefficient_x = 1e-05,
                                 thermal_expansion_coefficient_xSpecified = true,
                                 thermal_expansion_coefficient_y = 1e-05,
                                 thermal_expansion_coefficient_ySpecified = true,
                                 thermal_expansion_coefficient_z = 1e-05,
                                 thermal_expansion_coefficient_zSpecified = true,
                                 division_multiplication_factor = 1,
                                 division_multiplication_factorSpecified = true,
                                 strain_hardening_modulus = 270000,
                                 strain_hardening_modulusSpecified = true,
                                 }

                             }
                         },
                    stiffness_matrix_editable = false,
                    stiffness_matrix_editableSpecified = true,
                    stiffness_modification = false,
                    stiffness_modificationSpecified = true,
                    stiffness_modification_type = material_stiffness_modification_type.STIFFNESS_MODIFICATION_TYPE_DIVISION,
                    stiffness_modification_typeSpecified = true,
                };

                material materialReinforcementBars = new material
                {
                    no = 2,
                    name = "B550S(A)",
                    material_type = material_material_type.TYPE_REINFORCING_STEEL,
                };


                section sectionRectangle = new section
                {
                    no = 1,
                    material = materialConcrete.no,
                    materialSpecified = true,
                    type = section_type.TYPE_PARAMETRIC_MASSIVE_I,
                    typeSpecified = true,
                    parametrization_type = section_parametrization_type.PARAMETRIC_MASSIVE_I__MASSIVE_RECTANGLE__R_M1,
                    parametrization_typeSpecified = true,
                    b = 0.5,
                    bSpecified = true,
                    h = 1.0,
                    hSpecified = true,
                };

                try
                {
                    model.begin_modification("Material");
                    model.set_material(materialConcrete);
                    model.set_material(materialReinforcementBars);
                    model.set_material(materialUser);
                    model.set_material(materialUserThermal);
                    model.set_section(sectionRectangle);

                }
                catch (Exception exception)
                {
                    model.cancel_modification();
                    throw;
                }
                finally
                {
                    try
                    {
                        model.finish_modification();
                    }
                    catch (Exception exception)
                    {
                        model.reset();
                    }
                }

                #region design configurations
                concrete_design_uls_configuration concreteULSConfiguration = new concrete_design_uls_configuration()
                {
                    no = 2,
                    name = "ScriptedULSConfiguration",
                    user_defined_name_enabled = true,
                    user_defined_name_enabledSpecified = true,
                    assigned_to_all_members = true,
                    assigned_to_all_membersSpecified = true,
                    assigned_to_all_surfaces = true,
                    assigned_to_all_surfacesSpecified = true,

                };
                concrete_design_sls_configuration concreteSLSConfiguration = new concrete_design_sls_configuration()
                {
                    no = 2,
                    name = "ScriptedSLSConfiguration",
                    user_defined_name_enabled = true,
                    user_defined_name_enabledSpecified = true,
                    assigned_to_all_members = true,
                    assigned_to_all_membersSpecified = true,
                    assigned_to_all_surfaces = true,
                    assigned_to_all_surfacesSpecified = true,

                };
                try
                {
                    model.begin_modification("Set concrete design configurations");
                    model.set_concrete_design_uls_configuration(concreteULSConfiguration);
                    model.set_concrete_design_sls_configuration(concreteSLSConfiguration);
                }
                catch (Exception exception)
                {
                    model.cancel_modification();
                    throw;
                }
                finally
                {
                    try
                    {
                        model.finish_modification();
                    }
                    catch (Exception exception)
                    {
                        throw;

                    }
                }
                #endregion

                #region Concrete Design input data
                member_concrete_longitudinal_reinforcement_items_row longitudinalReinforcement = new member_concrete_longitudinal_reinforcement_items_row()
                {
                    no = 1,
                    row = new member_concrete_longitudinal_reinforcement_items()
                    {
                        rebar_type = rebar_type.REBAR_TYPE_SYMMETRICAL,
                        rebar_typeSpecified = true,
                        material = materialReinforcementBars.no,
                        materialSpecified = true,
                        bar_count_symmetrical = 3,
                        bar_count_symmetricalSpecified = true,
                        bar_diameter_symmetrical = 0.008,
                        bar_diameter_symmetricalSpecified = true,
                        span_position_reference_type = member_concrete_longitudinal_reinforcement_items_span_position_reference_type.LONGITUDINAL_REINFORCEMENT_ITEM_REFERENCE_START,
                        span_position_reference_typeSpecified = true,
                        // span_position_definition_format_type = member_concrete_longitudinal_reinforcement_items_span_position_definition_format_type.LONGITUDIANL_REINFORCEMENT_SPAN_DEFINITION_FORMAT_RELATIVE,
                        // span_position_definition_format_typeSpecified = true,
                        span_start_relative = 0.0,
                        span_start_relativeSpecified = true,
                        span_end_relative = 0.75,
                        span_end_relativeSpecified = true,
                        anchorage_start_anchor_type = anchorage_start_anchor_type.ANCHORAGE_TYPE_NONE,
                        anchorage_end_anchor_type = anchorage_end_anchor_type.ANCHORAGE_TYPE_NONE,
                    }

                };
                member_concrete_shear_reinforcement_spans_row shearReinforcement = new member_concrete_shear_reinforcement_spans_row()
                {
                    no = 1,
                    row = new member_concrete_shear_reinforcement_spans()
                    {
                        material = materialReinforcementBars.no,
                        stirrup_type = stirrup_type.STIRRUP_TYPE_FOUR_LEGGED_CLOSED_HOOK_135,
                        stirrup_distances = 0.3,
                        stirrup_diameter = 0.08,
                        span_start_relative = 0.0,
                        span_start_relativeSpecified = true,
                        span_end_relative = 0.75,
                        span_end_relativeSpecified = true,
                        span_position_reference_type = span_position_reference_type.SHEAR_REINFORCEMENT_SPAN_REFERENCE_START,
                        span_position_reference_typeSpecified = true,
                        span_position_definition_format_type = span_position_definition_format_type.SHEAR_REINFORCEMENT_SPAN_DEFINITION_FORMAT_RELATIVE,
                        span_position_definition_format_typeSpecified = true,
                    }
                };

                design_support design_Support = new design_support()
                {
                    no = 1,
                    type = design_support_type.DESIGN_SUPPORT_TYPE_CONCRETE,
                    typeSpecified = true,
                    user_defined_name_enabled = true,
                    user_defined_name_enabledSpecified = true,
                    name = "Concrete design support scripted",
                    comment = " scripted support",
                    activate_in_y = true,
                    activate_in_ySpecified = true,
                    activate_in_z = true,
                    activate_in_zSpecified = true,
                    consider_in_deflection_design_y = true,
                    consider_in_deflection_design_ySpecified = true,
                    consider_in_deflection_design_z = true,
                    concrete_monolithic_connection_z_enabled = true,
                    concrete_monolithic_connection_z_enabledSpecified = true,
                    concrete_ratio_of_moment_redistribution_z = 1,
                    concrete_ratio_of_moment_redistribution_zSpecified = true,
                    design_support_orientation_z = design_support_design_support_orientation_z.DESIGN_SUPPORT_ORIENTATION_ZAXIS_POSITIVE,
                    design_support_orientation_zSpecified = true,
                    direct_support_z_enabled = true,
                    direct_support_z_enabledSpecified = true,
                    inner_support_z_enabled = true,
                    inner_support_z_enabledSpecified = true,
                    support_depth_by_section_width_of_member_z_enabled = true,
                    support_depth_by_section_width_of_member_z_enabledSpecified = true,
                    support_width_z = 0.3,
                    support_width_zSpecified = true,
                    support_depth_by_section_width_of_member_y_enabled = true,
                    support_depth_by_section_width_of_member_y_enabledSpecified = true,
                };

                concrete_effective_lengths_factors_row factors = new concrete_effective_lengths_factors_row()
                {
                    no = 1,
                    row = new concrete_effective_lengths_factors()
                    {
                        flexural_buckling_y = 1,
                        flexural_buckling_ySpecified = true,
                        flexural_buckling_z = 1,
                        flexural_buckling_zSpecified = true,
                    }
                };
                concrete_effective_lengths_nodal_supports_row celNodalSupportsStart = new concrete_effective_lengths_nodal_supports_row()
                {
                    no = 1,
                    row = new concrete_effective_lengths_nodal_supports()
                    {
                        support_type = support_type.SUPPORT_TYPE_FIXED_IN_Z,
                        support_typeSpecified = true,
                        support_in_y_type = support_in_y_type.SUPPORT_STATUS_NO,
                        support_in_y_typeSpecified = true,
                        support_in_z = true,
                        support_in_zSpecified = true,
                        eccentricity_type = eccentricity_type.ECCENTRICITY_TYPE_NONE,
                        eccentricity_typeSpecified = true,
                        eccentricity_ez = 0,
                        eccentricity_ezSpecified = true,
                        restraint_about_x_type = restraint_about_x_type.SUPPORT_STATUS_NO,
                        restraint_about_x_typeSpecified = true,
                        restraint_about_z_type = restraint_about_z_type.SUPPORT_STATUS_NO,
                        restraint_about_z_typeSpecified = true,
                        restraint_spring_about_x = 0,
                        restraint_spring_about_xSpecified = true,
                        restraint_spring_about_z = 0,
                        restraint_spring_about_zSpecified = true,
                        restraint_spring_warping = 0,
                        restraint_spring_warpingSpecified = true,
                        restraint_warping_type = restraint_warping_type.SUPPORT_STATUS_NO,
                        restraint_warping_typeSpecified = true,
                        support_spring_in_y = 0,
                        support_spring_in_ySpecified = true,
                    }
                };
                concrete_effective_lengths_nodal_supports_row celNodalSupportsEnd = new concrete_effective_lengths_nodal_supports_row()
                {
                    no = 2,
                    row = new concrete_effective_lengths_nodal_supports()
                    {
                        support_type = support_type.SUPPORT_TYPE_FIXED_ALL,
                        support_typeSpecified = true,
                        support_in_y_type = support_in_y_type.SUPPORT_STATUS_YES,
                        support_in_y_typeSpecified = true,
                        support_in_z = true,
                        support_in_zSpecified = true,
                        eccentricity_type = eccentricity_type.ECCENTRICITY_TYPE_NONE,
                        eccentricity_typeSpecified = true,
                        eccentricity_ez = 0,
                        eccentricity_ezSpecified = true,
                        restraint_about_x_type = restraint_about_x_type.SUPPORT_STATUS_NO,
                        restraint_about_x_typeSpecified = true,
                        restraint_about_z_type = restraint_about_z_type.SUPPORT_STATUS_NO,
                        restraint_about_z_typeSpecified = true,
                        restraint_spring_about_x = 0,
                        restraint_spring_about_xSpecified = true,
                        restraint_spring_about_z = 0,
                        restraint_spring_about_zSpecified = true,
                        restraint_spring_warping = 0,
                        restraint_spring_warpingSpecified = true,
                        restraint_warping_type = restraint_warping_type.SUPPORT_STATUS_NO,
                        restraint_warping_typeSpecified = true,
                        support_spring_in_y = 0,
                        support_spring_in_ySpecified = true,
                    }
                };
                concrete_effective_lengths concrete_Effective_Lengths = new concrete_effective_lengths()
                {
                    no = 2,
                    flexural_buckling_about_y = true,
                    flexural_buckling_about_ySpecified = true,
                    flexural_buckling_about_z = true,
                    flexural_buckling_about_zSpecified = true,
                    structure_type_about_axis_y = concrete_effective_lengths_structure_type_about_axis_y.STRUCTURE_TYPE_UNBRACED,
                    structure_type_about_axis_ySpecified = true,
                    structure_type_about_axis_z = concrete_effective_lengths_structure_type_about_axis_z.STRUCTURE_TYPE_UNBRACED,
                    nodal_supports = new concrete_effective_lengths_nodal_supports_row[] { celNodalSupportsStart, celNodalSupportsEnd },
                    factors = new concrete_effective_lengths_factors_row[] { factors },
                    different_properties = true,
                    different_propertiesSpecified = true,
                    intermediate_nodes = false,
                    intermediate_nodesSpecified = true,


                };

                // concrete_durability concrete_Durability = model.get_concrete_durability(1);
                concrete_durability concrete_Durability = new concrete_durability()
                {
                    no = 1,
                    corrosion_induced_by_carbonation_enabled = true,
                    corrosion_induced_by_carbonation_enabledSpecified = true,
                    corrosion_induced_by_carbonation = concrete_durability_corrosion_induced_by_carbonation.CORROSION_INDUCED_BY_CARBONATION_TYPE_DRY_OR_PERMANENTLY_WET,
                    corrosion_induced_by_carbonationSpecified = true,
                    structural_class_type = concrete_durability_structural_class_type.STANDARD,
                    increase_design_working_life_from_50_to_100_years_enabled = false,
                    increase_design_working_life_from_50_to_100_years_enabledSpecified = true,
                    position_of_reinforcement_not_affected_by_construction_process_enabled = false,
                    position_of_reinforcement_not_affected_by_construction_process_enabledSpecified = true,
                    special_quality_control_of_production_enabled = false,
                    special_quality_control_of_production_enabledSpecified = true,
                    air_entrainment_of_more_than_4_percent_enabled = false,
                    air_entrainment_of_more_than_4_percent_enabledSpecified = true,
                    additional_protection_enabled = false,
                    additional_protection_enabledSpecified = true,
                    allowance_of_deviation_type = concrete_durability_allowance_of_deviation_type.STANDARD,
                    allowance_of_deviation_typeSpecified = true,

                };

                //reinforcement_direction Reinforcement_Direction = model.get_reinforcement_direction(1);
                reinforcement_direction Reinforcement_Direction = new reinforcement_direction()
                {
                    no = 1,
                    reinforcement_direction_type = reinforcement_direction_reinforcement_direction_type.REINFORCEMENT_DIRECTION_TYPE_FIRST_REINFORCEMENT_IN_X,
                    reinforcement_direction_typeSpecified = true,

                };

                // surface_reinforcement Surface_Reinforcement = model.get_surface_reinforcement(1);
                surface_reinforcement Surface_Reinforcement = new surface_reinforcement()
                {
                    no = 1,
                    location_type = surface_reinforcement_location_type.LOCATION_TYPE_ON_SURFACE,
                    location_typeSpecified = true,
                    material = materialReinforcementBars.no,
                    materialSpecified = true,
                    reinforcement_type = surface_reinforcement_reinforcement_type.REINFORCEMENT_TYPE_REBAR,
                    reinforcement_typeSpecified = true,
                    rebar_diameter = 0.01,
                    rebar_diameterSpecified = true,
                    rebar_spacing = 0.15,
                    rebar_spacingSpecified = true,
                    additional_transverse_reinforcement_enabled = false,
                    additional_transverse_reinforcement_enabledSpecified = true,
                    additional_offset_to_concrete_cover_top = 0.0,
                    additional_offset_to_concrete_cover_topSpecified = true,
                    additional_offset_to_concrete_cover_bottom = 0.0,
                    additional_offset_to_concrete_cover_bottomSpecified = true,
                    alignment_bottom_enabled = true,
                    alignment_bottom_enabledSpecified = true,
                    alignment_top_enabled = false,
                    alignment_top_enabledSpecified = true,
                    reinforcement_direction_type = surface_reinforcement_reinforcement_direction_type.REINFORCEMENT_DIRECTION_TYPE_IN_DESIGN_REINFORCEMENT_DIRECTION,
                    reinforcement_direction_typeSpecified = true,
                    design_reinforcement_direction = surface_reinforcement_design_reinforcement_direction.DESIGN_REINFORCEMENT_DIRECTION_A_S_1,
                    design_reinforcement_directionSpecified = true,
                };

                surface_reinforcement Surface_ReinforcementMesh = new surface_reinforcement()
                {
                    no = 2,
                    location_type = surface_reinforcement_location_type.LOCATION_TYPE_ON_SURFACE,
                    location_typeSpecified = true,
                    material = materialReinforcementBars.no,
                    materialSpecified = true,
                    reinforcement_type = surface_reinforcement_reinforcement_type.REINFORCEMENT_TYPE_MESH,
                    reinforcement_typeSpecified = true,
                    mesh_name = "Q188A",
                    mesh_product_range = surface_reinforcement_mesh_product_range.MESHSTANDARD_GERMANY_2008_01_01,
                    mesh_shape = surface_reinforcement_mesh_shape.MESHSHAPE_Q_MESH,
                    additional_offset_to_concrete_cover_top = 0.0,
                    additional_offset_to_concrete_cover_topSpecified = true,
                    additional_offset_to_concrete_cover_bottom = 0.0,
                    additional_offset_to_concrete_cover_bottomSpecified = true,
                    alignment_bottom_enabled = false,
                    alignment_bottom_enabledSpecified = true,
                    alignment_top_enabled = true,
                    alignment_top_enabledSpecified = true,
                    reinforcement_direction_type = surface_reinforcement_reinforcement_direction_type.REINFORCEMENT_DIRECTION_TYPE_IN_DESIGN_REINFORCEMENT_DIRECTION,
                    reinforcement_direction_typeSpecified = true,
                    design_reinforcement_direction = surface_reinforcement_design_reinforcement_direction.DESIGN_REINFORCEMENT_DIRECTION_A_S_1,
                    design_reinforcement_directionSpecified = true,
                };

                try
                {
                    model.begin_modification("Set concrete design input data");
                    model.set_design_support(design_Support);
                    model.set_concrete_effective_lengths(concrete_Effective_Lengths);
                    model.set_concrete_durability(concrete_Durability);
                    model.set_reinforcement_direction(Reinforcement_Direction);
                    model.set_surface_reinforcement(Surface_Reinforcement);
                    model.set_surface_reinforcement(Surface_ReinforcementMesh);

                }
                catch (Exception exception)
                {
                    model.cancel_modification();
                    throw;
                }
                finally
                {
                    try
                    {
                        model.finish_modification();
                    }
                    catch (Exception exception)
                    {
                        throw;

                    }
                }
                #endregion

                node n1 = new()
                {
                    no = 1,
                    coordinates = new vector_3d() { x = 0.0, y = 0.0, z = 0.0 },
                    coordinate_system_type = node_coordinate_system_type.COORDINATE_SYSTEM_CARTESIAN,
                    coordinate_system_typeSpecified = true,
                    comment = "concrete part"
                };

                node n2 = new()
                {
                    no = 2,
                    coordinates = new vector_3d() { x = 5.0, y = 0.0, z = 0.0 },
                    coordinate_system_type = node_coordinate_system_type.COORDINATE_SYSTEM_CARTESIAN,
                    coordinate_system_typeSpecified = true,
                    comment = "concrete part"
                };

                line line = new()
                {
                    no = 1,
                    definition_nodes = new int[] { n1.no, n2.no },
                    comment = "lines for beams",
                    type = line_type.TYPE_POLYLINE,
                    typeSpecified = true,
                };
                // create member
                member member = new()
                {
                    no = 1,
                    line = line.no,
                    lineSpecified = true,
                    section_start = sectionRectangle.no,
                    section_startSpecified = true,
                    section_end = sectionRectangle.no,
                    section_endSpecified = true,
                    comment = "concrete beam",
                    concrete_durability = concrete_Durability.no,
                    concrete_durabilitySpecified = true,
                    concrete_longitudinal_reinforcement_items = new member_concrete_longitudinal_reinforcement_items_row[] { longitudinalReinforcement },
                    concrete_shear_reinforcement_spans = new member_concrete_shear_reinforcement_spans_row[] { shearReinforcement },
                    concrete_effective_lengths = concrete_Effective_Lengths.no,
                    concrete_effective_lengthsSpecified = true,
                    member_concrete_design_uls_configuration = 1,
                    member_concrete_design_uls_configurationSpecified = true,
                    member_concrete_design_sls_configuration = 1,
                    member_concrete_design_sls_configurationSpecified = true,
                    deflection_check_direction = member_deflection_check_direction.DEFLECTION_CHECK_DIRECTION_LOCAL_AXIS_Z_AND_Y,
                    deflection_check_directionSpecified = true,
                    deflection_check_displacement_reference = member_deflection_check_displacement_reference.DEFLECTION_CHECK_DISPLACEMENT_REFERENCE_DEFORMED_UNDEFORMED_SYSTEM,
                    deflection_check_displacement_referenceSpecified = true,
                    design_support_on_member_start = design_Support.no,
                    design_support_on_member_startSpecified = true,
                    design_support_on_member_end = design_Support.no,
                    design_support_on_member_endSpecified = true,
                };

                nodal_support support = new()
                {
                    no = 1,
                    nodes = new int[] { n1.no },
                    spring = new vector_3d() { x = double.PositiveInfinity, y = double.PositiveInfinity, z = double.PositiveInfinity },
                    rotational_restraint = new vector_3d() { x = double.PositiveInfinity, y = double.PositiveInfinity, z = double.PositiveInfinity }
                };

                try
                {
                    model.begin_modification("Geometry");
                    model.set_node(n1);
                    model.set_node(n2);
                    model.set_line(line);
                    model.set_member(member);
                    model.set_nodal_support(support);
                }
                catch (Exception exception)
                {
                    model.cancel_modification();
                    throw;
                }
                finally
                {
                    try
                    {
                        model.finish_modification();
                    }
                    catch (Exception exception)
                    {
                        model.reset();
                    }
                }



                static_analysis_settings analysis = new()
                {
                    no = 1,
                    analysis_type = static_analysis_settings_analysis_type.GEOMETRICALLY_LINEAR,
                    analysis_typeSpecified = true,
                };


                load_case selfWeightLC = new()
                {
                    no = 1,
                    name = "SelfWeight",
                    static_analysis_settings = analysis.no,
                    analysis_type = load_case_analysis_type.ANALYSIS_TYPE_STATIC,
                    analysis_typeSpecified = true,
                    static_analysis_settingsSpecified = true,
                    self_weight_active = true,
                    self_weight_activeSpecified = true,
                    self_weight_factor_z = 1.0,
                    self_weight_factor_zSpecified = true,
                    action_category = "ACTION_CATEGORY_PERMANENT_G",
                    calculate_critical_load = true,
                    calculate_critical_loadSpecified = true,
                    stability_analysis_settings = analysis.no,
                    stability_analysis_settingsSpecified = true,
                };

                load_case lcData = new()
                {
                    no = 2,
                    name = "My load case",
                    self_weight_active = false,
                    self_weight_activeSpecified = true,
                    static_analysis_settings = analysis.no,
                    static_analysis_settingsSpecified = true,
                    analysis_type = load_case_analysis_type.ANALYSIS_TYPE_STATIC,
                    analysis_typeSpecified = true,
                    action_category = "ACTION_CATEGORY_PERMANENT_IMPOSED_GQ",
                };

                design_situation design_Situation = new design_situation()
                {
                    no = 1,
                    name = "ScriptedDS",
                    user_defined_name_enabled = true,
                    user_defined_name_enabledSpecified = true,
                    design_situation_type = "DESIGN_SITUATION_TYPE_EQU_PERMANENT_AND_TRANSIENT",
                    is_generated = false,
                    is_generatedSpecified = true,
                    consider_inclusive_exclusive_load_cases = true,
                    consider_inclusive_exclusive_load_casesSpecified = true,
                };



                load_combination_items_row load_Combination_SW = new load_combination_items_row()
                {
                    no = 1,
                    row = new load_combination_items()
                    {
                        load_case = 1,
                        load_caseSpecified = true,
                        factor = 1.35,
                        factorSpecified = true,
                    }

                };

                load_combination_items_row load_Combination_lcData = new load_combination_items_row()
                {
                    no = 2,
                    row = new load_combination_items()
                    {
                        load_case = 2,
                        load_caseSpecified = true,
                        factor = 1.5,
                        factorSpecified = true,
                    }

                };
                load_combination_items_row[] loadCombinationItems = new load_combination_items_row[] { load_Combination_SW, load_Combination_lcData };



                load_combination load_Combination = new load_combination()
                {
                    no = 1,
                    name = "ScriptedCombination",
                    user_defined_name_enabled = true,
                    user_defined_name_enabledSpecified = true,
                    to_solve = true,
                    to_solveSpecified = true,
                    analysis_type = load_combination_analysis_type.ANALYSIS_TYPE_STATIC,
                    analysis_typeSpecified = true,
                    items = loadCombinationItems,
                    design_situation = 1,
                    design_situationSpecified = true,
                };
                try
                {
                    model.begin_modification("Load");
                    model.set_static_analysis_settings(analysis);
                    model.set_load_case(selfWeightLC);
                    model.set_load_case(lcData);

                    model.set_design_situation(design_Situation);
                    model.set_load_combination(load_Combination);

                }
                catch (Exception exception)
                {
                    model.cancel_modification();
                    throw;
                }
                finally
                {
                    try
                    {
                        model.finish_modification();
                    }
                    catch (Exception exception)
                    {
                        model.reset();
                    }
                }

                member_load memberLoadonBeam = new()
                {
                    no = 1,
                    members_string = member.no.ToString(),
                    members = new int[] { member.no },
                    load_distribution = member_load_load_distribution.LOAD_DISTRIBUTION_TRAPEZOIDAL,
                    load_distributionSpecified = true,
                    magnitude = 30000,
                    magnitudeSpecified = true,
                    magnitude_1 = 10000,
                    magnitude_1Specified = true,
                    magnitude_2 = 20000,
                    magnitude_2Specified = true,
                    load_is_over_total_length = true,
                    load_is_over_total_lengthSpecified = true,
                };
                try
                {
                    model.begin_modification("Set loads");
                    model.set_member_load(lcData.no, memberLoadonBeam);
                }
                catch (Exception exception)
                {
                    model.cancel_modification();
                    throw;
                }
                finally
                {
                    try
                    {
                        model.finish_modification();
                    }
                    catch (Exception exception)
                    {
                        model.reset();
                    }
                }


                #region generate mesh and get mesh statistics
                calculation_message[] meshGenerationMessage = model.generate_mesh(true);
                if (meshGenerationMessage.Length != 0)
                {
                }
                mesh_statistics_type mesh_Statistics = model.get_mesh_statistics();
                Console.WriteLine("Number of mesh nodes: " + mesh_Statistics.node_elements);
                Console.WriteLine("Number of 1D elements: " + mesh_Statistics.member_1D_finite_elements);
                Console.WriteLine("Number of surface element: " + mesh_Statistics.surface_2D_finite_elements);
                Console.WriteLine("Number of volume elements: " + mesh_Statistics.solid_3D_finite_elements);

                #endregion


                calculation_result calculationResult = model.calculate_all(true);
                if (!calculationResult.succeeded || !String.IsNullOrEmpty(calculationResult.messages) || calculationResult.errors_and_warnings.Any())
                {
                    Console.Write("Calculation is not finished successfully");
                    if (!String.IsNullOrEmpty(calculationResult.messages))
                    {
                        Console.Write(calculationResult.messages);
                    }
                    if (calculationResult.errors_and_warnings.Any())
                    {
                        foreach (calculation_message message in calculationResult.errors_and_warnings)
                        {
                            Console.Write($"{message.message_type.ToString()}  {message.message} {(message.input_field != null ? message.input_field : "")} {(message.@object != null ? message.@object : "")} {(message.current_value != null ? message.current_value : "")} {message.result.ToString()}");
                        }

                    }
                    if (!String.IsNullOrEmpty(calculationResult.messages) && !calculationResult.errors_and_warnings.Any())
                    {
                        errors_row[] errors = model.get_calculation_errors();
                        if (errors.Any())
                        {
                            foreach (errors_row error in errors)
                            {
                                Console.Write($"{error.no} {error.description}  {error.row.analysis_type} {error.row.description} {error.row.error_or_warning_number} {error.row.@object}");
                            }
                        }
                    }
                }

                #region Results
                bool modelHasAnyResults = model.has_any_results();

                if (modelHasAnyResults)
                {
                    Console.WriteLine("Model has results");
                }
                else
                {
                    Console.WriteLine("Model has no results");
                }

                bool modelHasLC2Calcuolated = model.has_results(case_object_types.E_OBJECT_TYPE_LOAD_CASE, lcData.no);
                if (modelHasLC2Calcuolated)
                {
                    Console.WriteLine("Model has LC2 results");
                }
                else
                {
                    Console.WriteLine("Model has no LC2 results");
                }

                model.use_detailed_member_results(true); // results along the length of the member, by default false -> results just at the begingign and end of the member + exteremes

                members_internal_forces_row[] internalForcesMember1 = model.get_results_for_members_internal_forces(case_object_types.E_OBJECT_TYPE_LOAD_CASE, lcData.no, member.no);
                Console.WriteLine("Internal forces for member");
                foreach (var item in internalForcesMember1)
                {
                    Console.WriteLine("Row no {0}\t Description {1}", item.no, item.description);
                    Console.WriteLine("Node {0}\t Location {1}\t Location flags {2}\t Internal force label {3}\t Specification {4}", item.row.node_number != null ? item.row.node_number?.value : "NAN", item.row.location, item.row.location_flags, item.row.internal_force_label, item.row.specification);
                    Console.WriteLine("N {0}\t Vy {1}\t Vz {2}\t Mx {3}\t My {4}\t Mz {5}\t", item.row.internal_force_n.ToString(), item.row.internal_force_vy.ToString(), item.row.internal_force_vz.ToString(), item.row.internal_force_mt.ToString(), item.row.internal_force_my.ToString(), item.row.internal_force_mz.ToString());

                }

                Console.WriteLine("Global deformations for member");
                members_global_deformations_row[] globalDeformationsMember1 = model.get_results_for_members_global_deformations(case_object_types.E_OBJECT_TYPE_LOAD_CASE, lcData.no, member.no);
                foreach (var item in globalDeformationsMember1)
                {
                    Console.WriteLine("Row no {0}\t Description {1}", item.no, item.description);
                    Console.WriteLine("Node {0}\t Location {1}\t Location flags {2}\t Deformation label {3}\t Specification {4}", item.row.node_number != null ? item.row.node_number.value : "NAN", item.row.location, item.row.location_flags, item.row.deformation_label, item.row.section);
                    Console.WriteLine("ux {0}\t uy {1}\t uz {2}\t utot {3}\t rx {4}\t ry {5}\t rz {6}\t warping {6}\t", item.row.displacement_x.ToString(), item.row.displacement_y.ToString(), item.row.displacement_z.ToString(), item.row.displacement_absolute.ToString(), item.row.rotation_x.ToString(), item.row.rotation_y.ToString(), item.row.rotation_z.ToString(), item.row.warping.ToString());

                }

                nodes_deformations_row[] nodeDeformations = model.get_results_for_nodes_deformations(case_object_types.E_OBJECT_TYPE_LOAD_CASE, lcData.no, 0);//all nodes -> 0
                Console.WriteLine("Node deformations");
                foreach (var item in nodeDeformations)
                {
                    Console.WriteLine("Row no {0}\t Description {1} node comment {2}", item.no, item.description, item.row.specification);
                    Console.WriteLine("ux {0}\t uy {1}\t uz {2}\t utot {3}\t rx {4}\t ry {5}\t rz {6}\t", item.row.displacement_x.ToString(), item.row.displacement_y.ToString(), item.row.displacement_z.ToString(), item.row.displacement_absolute.ToString(), item.row.rotation_x.ToString(), item.row.rotation_y.ToString(), item.row.rotation_z.ToString());

                }
                nodes_support_forces_row[] nodeReactions = model.get_results_for_nodes_support_forces(case_object_types.E_OBJECT_TYPE_LOAD_CASE, lcData.no, 0);//all nodes -> 0
                Console.WriteLine("Node reactions");
                foreach (var item in nodeReactions)
                {
                    Console.WriteLine("Row no {0}\t Description {1}", item.no, item.description);
                    Console.WriteLine("note corresponding loading {0}\t px {1}\t py {2}\t pz {3}\t mx {4}\t my {5}\t mz {6}\t label {7}\t", item.row.node_comment_corresponding_loading?.ToString(), item.row.support_force_p_x?.value.ToString(), item.row.support_force_p_y?.value.ToString(), item.row.support_force_p_z?.value.ToString(), item.row.support_moment_m_x?.value.ToString(), item.row.support_moment_m_y.ToString(), item.row.support_moment_m_z.ToString(), DecodeHtmlString(item.row.support_forces_label));

                }
                #endregion

                #region Generate part list
                model.generate_parts_lists();
                parts_list_all_by_material_row[] partListByAllMaterial = model.get_parts_list_all_by_material();
                foreach (var item in partListByAllMaterial)
                {
                    if (!item.description.Contains("Total"))
                    {//material no
                        Console.WriteLine("Material no: {0}\t Material name: {1}\t object type: {2}\t coating:{3}\t volume: {4}\t mass: {5}", item.description, item.row.material_name, item.row.object_type, item.row.total_coating, item.row.volume, item.row.mass);
                    }
                    else
                    {//material no total
                        Console.WriteLine("Material total\t \t \t coating:{0}\t volume: {1}\t mass: {2}", item.row.total_coating, item.row.volume, item.row.mass);
                    }

                }
                Console.WriteLine("Members: ");
                parts_list_members_by_material_row[] partListMemberByMaterial = model.get_parts_list_members_by_material();
                foreach (var item in partListMemberByMaterial)
                {
                    if (!item.description.Contains("Total"))
                    {
                        Console.WriteLine("Material no: {0}\t Material name: {1}\t section: {2}\t members no:{3}\t quantity: {4}\t length: {5}\t unit surface area: {6}\t volume: {7}\t unit mass: {8}\t member mass: {9}\t total length: {10}\t total surface area: {11}\t total volume:{12}\t total mass:{13}",
                        item.description, item.row.material_name, item.row.section_name, item.row.members_no, item.row.quantity, item.row.length, item.row.unit_surface_area, item.row.volume, item.row.unit_mass, item.row.member_mass, item.row.total_length, item.row.total_surface_area, item.row.total_volume, item.row.total_mass);
                    }
                    else
                    {
                        Console.WriteLine("Total \t \t \t \t quantity: {4}\t length: {5}\t unit surface area: {6}\t volume: {7}\t unit mass: {8}\t member mass: {9}\t total length: {10}\t total surface area: {11}\t total volume:{12}\t total mass:{13}",
                                            item.description, item.row.material_name, item.row.section_name, item.row.members_no, item.row.quantity, item.row.length, item.row.unit_surface_area, item.row.volume, item.row.unit_mass, item.row.member_mass, item.row.total_length, item.row.total_surface_area, item.row.total_volume, item.row.total_mass);

                    }

                }
                #endregion


                object_location[] objectLocationSelectedMembers = new object_location[]{
                     new object_location(){
                         no = member.no,
                         parent_no = 0,
                         parent_noSpecified = true,
                         type = object_types.E_OBJECT_TYPE_MEMBER
                     }
                 };
                results_for_concrete_design_overview_errors_and_warnings_row[] designOverviewConcrete = model.get_results_for_concrete_design_overview_errors_and_warnings(objectLocationSelectedMembers);
                foreach (var item in designOverviewConcrete)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", item.row.addon, item.row.object_type, item.row.objects_no_string, item.row.loading, item.row.design_situation,
                    item.row.loading, item.row.design_ratio, item.row.description);
                }

                results_for_concrete_design_overview_not_valid_deactivated_row[] designOverviewConcreteNotActivated = model.get_results_for_concrete_design_overview_not_valid_deactivated(objectLocationSelectedMembers);
                foreach (var item in designOverviewConcreteNotActivated)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", item.row.object_type, item.row.objects_string, item.row.error_type, item.row.description);
                }
                //by locations
                results_for_concrete_design_design_ratios_members_by_location_row[] designRationsConcreteMember = model.get_results_for_concrete_design_design_ratios_members_by_location(objectLocationSelectedMembers);
                foreach (var item in designRationsConcreteMember)
                { 
                    Console.WriteLine("Row No: {0}\t Description: {1}", item.no, item.description);
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t", item.row.member?.value, item.row.location?.value, item.row.design_situation?.value, item.row.loading?.value, item.row.design_ratio?.value, item.row.design_check_type?.value, DecodeHtmlString(item.row.design_check_formula?.value), item.row.design_check_description?.value);
                }

                
                results_for_concrete_design_governing_internal_forces_by_member_row[] concreteGovInternalForcesByMember = model.get_results_for_concrete_design_governing_internal_forces_by_member(objectLocationSelectedMembers);
                foreach (var item in concreteGovInternalForcesByMember)
                {
                    Console.WriteLine("Row No: {0}\t Description: {1}", item.no, item.description);
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t", item.row.member?.value, item.row.location?.value, item.row.design_situation?.value, item.row.loading?.value, item.row.design_ratio
                    ?.value, item.row.design_check_type?.value, DecodeHtmlString(item.row.design_check_formula?.value), item.row.design_check_description?.value,
                    item.row.force_n?.value, item.row.force_vy?.value, item.row.force_vz?.value, item.row.moment_mt?.value, item.row.moment_my?.value, item.row.moment_mz?.value);
                }
                results_for_concrete_design_required_reinforcement_area_on_members_by_section_row[] requiredReinforcement = model.get_results_for_concrete_design_required_reinforcement_area_on_members_by_section(objectLocationSelectedMembers);
                foreach (var item in requiredReinforcement)
                {
                    Console.WriteLine("Row No: {0}\t Description: {1}", item.no, item.description);
                }

                model.close_connection();
                model_name_and_index[] modelNamesAndIndexes = application.get_model_list_with_indexes();
                var query = from item in modelNamesAndIndexes where item.name == modelName select item.index;
                int i = query.FirstOrDefault();
                application.close_model(i, false);//close model
                
                //  application.close_application();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }

}