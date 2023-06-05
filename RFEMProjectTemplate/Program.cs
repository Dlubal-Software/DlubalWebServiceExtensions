﻿using System;
using System.IO;
using Dlubal.WS.Rfem6.Application;
using ApplicationClient = Dlubal.WS.Rfem6.Application.RfemApplicationClient;
using Dlubal.WS.Rfem6.Model;
using ModelClient = Dlubal.WS.Rfem6.Model.RfemModelClient;
using System.ServiceModel;

namespace Cantilever
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
                };

                return binding;
            }
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

                material materialConcrete = new material
                {
                    no = 1,
                    name = "C20/25 | EN 1992-1-1:2004/A1:2014"
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
                    name = "R_M1 0.5/1.0", // width/height as in RFEM
                };

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
                    comment = "concrete beam"
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
                    model.set_material(materialConcrete);
                    model.set_section(sectionRectangle);
                    model.set_node(n1);
                    model.set_node(n2);
                    model.set_line(line);
                    model.set_member(member);
                    model.set_nodal_support(support);
                }
                catch (Exception exception)
                {
                    model.cancel_modification();
                    Console.WriteLine("Something happen when creation of geometry" + exception.Message);
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
                        Console.WriteLine("Something happen when finishing creation of geometry" + exception.Message);
                        model.reset();
                        throw;
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
                    Console.WriteLine("Something happen when creation of load" + exception.Message);
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
                        Console.WriteLine("Something wrong in finish modification of load\n" + exception.Message + "\n");
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
                    Console.WriteLine("Something happen when creation of load" + exception.Message);
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
                        Console.WriteLine("Something wrong in finish modification of load\n" + exception.Message + "\n");
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

                calculation_message[] calculationMesages = model.calculate_all(true);
                if (calculationMesages.Length != 0)
                {
                }
                else
                {
                    Console.WriteLine("Calculation finished successfully");
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
                    Console.WriteLine("Node {0}\t Location {1}\t Location flags {2}\t Internal force label {3}\t Specification {4}", item.row.node_number != null ? item.row.node_number.value : "NAN", item.row.location, item.row.location_flags, item.row.internal_force_label, item.row.specification);
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
                    Console.WriteLine("note corresponding loading {0}\t px {1}\t py {2}\t pz {3}\t mx {4}\t my {5}\t mz {6}\t label {7}\t", item.row.node_comment_corresponding_loading.ToString(), item.row.support_force_p_x.value.ToString(), item.row.support_force_p_y.value.ToString(), item.row.support_force_p_z.value.ToString(), item.row.support_moment_m_x.value.ToString(), item.row.support_moment_m_y.ToString(), item.row.support_moment_m_z.ToString(), item.row.support_forces_label);

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
                application.close_model(0, false);//close model
                                                  //  application.close_application();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Console.WriteLine("Stopped program because of exception :" + ex.Message);
            }
        }
    }

}