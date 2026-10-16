using Common.Can;
using Common.UDS;
using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllData;

namespace DetectionProcess.LT
{
    public class EEPROMTest : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------EEPROM测试------");

            if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                var wlPar = pars.FirstOrDefault();
                AddModelAction(new TestResult()
                {
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    ChildrenItem = wlPar.ChildrenItem,
                    Extend1 = "1",
                    MeasureVal = "1",
                    Result = true,
                    QualifiedValue = ">0"
                });
                CommonData.TestCount++;
                return true;
            }
           
            var par = pars.FirstOrDefault();
            if (par == null)
            {
                //LogHelper.ShowMsg("EEPROM测试参数未配置");
                AddModelAction(new TestResult() { ChildrenItem = "EEPROM测试失败" });
                throw new Exception("EEPROM测试参数未配置");
            }
            NewCanHelper.MOSControlVoltageEnable(true, false);
            TestResult model = new TestResult();
            model.StartTime = DateTime.Now;

            bool b = false;

            //if (ConfigHelper.UdsParameters.jiesuo == "XP-12V")
            //{
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();

            //}



            //   AddModelAction(new TestResult() { ChildrenItem = "复位", Extend1 = "-1" });


            if ( ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03")
            {
                var temp = PlayUDSHelper.EEPROM();

                if (temp == null)
                {
                    LogHelper.ShowMsg("读取快照失败");
                    throw new Exception("读取快照失败");
                }
                model.EndTime = DateTime.Now;
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.MeasureVal = "";

                if (temp != null)
                {
                    //foreach (var item in temp)
                    //{
                    //    model.MeasureVal += item.ToString("X2") + " ";
                    //}
                    model.MeasureVal = temp[4].ToString();
                }
                model.Result = temp[4] <= 1;
                model.QualifiedValue = "0`1";
                //model.MeasureVal = "0";
                AddModelAction(model);

            }
            else if (ConfigHelper.UdsParameters.jiesuo == "M18_3")
            {
                var temp = PlayUDSHelper.EEPROM();

                if (temp == null)
                {
                    LogHelper.ShowMsg("读取快照失败");
                    throw new Exception("读取快照失败");
                }
                model.EndTime = DateTime.Now;
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.MeasureVal = "";

                if (temp != null)
                {
                    //foreach (var item in temp)
                    //{
                    //    model.MeasureVal += item.ToString("X2") + " ";
                    //}
                    model.MeasureVal = ((temp[4] & 0x80)>>7).ToString();
                }
                model.Result = ((temp[4] & 0x80) >> 7) == 1;
                model.QualifiedValue = "1";
                //model.MeasureVal = "0";
                AddModelAction(model);

            }
            else if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" || 
                ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A" || ConfigHelper.UdsParameters.jiesuo == "AVATR_E15" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A" || ConfigHelper.UdsParameters.jiesuo == "DF_DH1")
            {

                var temp1 = PlayUDSHelper.GETEEPROM();

                byte temp = 0;
             
                
                    CommonMethod.Sleep(1000);
                    byte[] bytes = new byte[] { 0x04, temp1[5], temp1[6], temp1[7], 0x01 };

                    temp = PlayUDSHelper.EEPROMWL(bytes);
               



                model.EndTime = DateTime.Now;
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.MeasureVal = "";

                model.MeasureVal = temp.ToString();

                model.Result = temp != 0;
                model.QualifiedValue = ">0";
                //model.MeasureVal = "0";
                AddModelAction(model);

            }
            else if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216")
            {
                var temp = PlayUDSHelper.EEPROMHQ();

                model.EndTime = DateTime.Now;
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.MeasureVal = "";

                model.MeasureVal = temp.ToString();

                model.Result = temp != 0;
                model.QualifiedValue = ">0";
                //model.MeasureVal = "0";
                AddModelAction(model);

            }
            else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
            {
                var temp = PlayUDSHelper.EEPROM();
                if (temp == null)
                {
                    LogHelper.ShowMsg("读取快照失败");
                    throw new Exception("读取快照失败");
                }
                model.EndTime = DateTime.Now;
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.MeasureVal = "";
                if (temp != null)
                {
                    //foreach (var item in temp)
                    //{
                    //    model.MeasureVal += item.ToString("X2") + " ";
                    //}
                    model.MeasureVal = temp[4].ToString();
                }
                model.Result = temp[4] <= 1;
                model.QualifiedValue = "0`1";
                AddModelAction(model);

            }
            else
            {
               
                var temp = PlayUDSHelper.EEPROM();

                if (temp == null)
                {
                    LogHelper.ShowMsg("读取快照失败");
                    throw new Exception("读取快照失败");
                }
                model.EndTime = DateTime.Now;
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.MeasureVal = "";
                if (ConfigHelper.UdsParameters.jiesuo == "JAC_X6")
                {
                    if (temp.Count > 31)
                    {
                        model.MeasureVal = temp[31].ToString();
                        model.Result = temp[31] > 1;
                        model.QualifiedValue = ">0";
                    }
                    else
                    {
                        LogHelper.ShowMsg("读取快照位数不够");
                        throw new Exception("读取快照失败");
                    }
                    
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "XP_12V")
                {
                    if (temp != null && temp[1] == 80)
                    {
                        CommonMethod.Sleep(200);
                        temp = PlayUDSHelper.EEPROM2();
                        if (temp != null)
                        {
                            model.MeasureVal = temp[1].ToString("X2");
                            model.Result = temp[1] == 103;
                            model.QualifiedValue = "67";
                        }
                        else
                        {
                            LogHelper.ShowMsg("读取快照位数不够");
                            throw new Exception("读取快照失败");
                        }
                    }
                    else
                    {
                        LogHelper.ShowMsg("读取快照位数不够");
                        throw new Exception("读取快照失败");
                    }

                }
                else
                {
                    if (temp != null)
                    {
                        //foreach (var item in temp)
                        //{
                        //    model.MeasureVal += item.ToString("X2") + " ";
                        //}
                        model.MeasureVal = temp[2].ToString();
                    }
                    model.Result = temp[2] <= 1;
                    model.QualifiedValue = "<=1";
                }
                    
                
                
                //model.MeasureVal = "0";
                AddModelAction(model);
            }

            if (PlayUDSHelper.Reset() == false)
            {
                //LogHelper.ShowMsg("复位失败");
                AddModelAction(new TestResult() { ChildrenItem = "复位失败" });
                throw new Exception($"复位失败");
            }
           

            NewCanHelper.WakeOrSleep(true);
            CommonMethod.Sleep(1000);
            CommonMethod.Unlock();
            CommonData.TestCount++;
            return true;
        }
    }
}
