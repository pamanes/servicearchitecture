using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// Class used to pass parameters to thread calls
    /// </summary>
    public class MyParams
    {
        /// <summary>
        /// 
        /// </summary>
        public int ThreadNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DeptValue { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thread_num"></param>
        /// <param name="dept_value"></param>
        public MyParams(int thread_num, string dept_value)
        {
            ThreadNum = thread_num;
            DeptValue = dept_value;
        }
    }

    public class UpdateDepartmentParams
    {
        public int ThreadNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DepartmentSid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DeptValue { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thread_num"></param>
        /// <param name="dept_value"></param>
        public UpdateDepartmentParams(int thread_num, int department_sid, string dept_value)
        {
            ThreadNum = thread_num;
            DepartmentSid = department_sid;
            DeptValue = dept_value;
        }
    }
    [TestClass]
    public class ComputerDetailRepo
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        volatile bool ErrorOnThread;
        /// </summary>
        [TestMethod]
        public void DepartmentAdd()
        {
            Thread[] threads = new Thread[12];

            //run it 30 times.
            for (int x = 0; x < 30; x++)
            {
                //get a unique value per run for computer detail
                string deptValue = DateTime.Now.Ticks.ToString();

                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(new ParameterizedThreadStart(CreateDepartment));
                }

                for (int i = 0; i < threads.Length; i++)
                {
                    Thread thread = threads[i];
                    MyParams p2 = new MyParams(i, deptValue);
                    thread.Start(p2);

                }

                //join all of them (wait)            
                foreach (Thread thread in threads)
                {
                    thread.Join();
                }

                if (ErrorOnThread)
                    Assert.Fail("Dupe Test failed");
            }
        }
        [TestMethod]
        public void DepartmentUpdate()
        {
            Thread[] threads = new Thread[1];
            DataSet ds = GetTopTwoDepartments();
            //run it 30 times.
            for (int x = 0; x < 30; x++)
            {
                //get a unique value per run for computer detail
                string deptValue = DateTime.Now.Ticks.ToString();

                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(new ParameterizedThreadStart(UpdateDepartment));
                }

                for (int i = 0; i < threads.Length; i++)
                {
                    Thread thread = threads[i];
                    UpdateDepartmentParams p2 = new UpdateDepartmentParams
                    (
                        i, 
                        (int)ds.Tables[0].Rows[0]["Department_SID"], 
                        (string)ds.Tables[0].Rows[1]["Department"]
                    );
                    thread.Start(p2);
                }

                //join all of them (wait)            
                foreach (Thread thread in threads)
                {
                    thread.Join();
                }
                threads = new Thread[1];
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(new ParameterizedThreadStart(UpdateDepartment));
                }
                for (int i = 0; i < threads.Length; i++)
                {
                    Thread thread = threads[i];
                    UpdateDepartmentParams p2 = new UpdateDepartmentParams
                    (
                        i,
                        (int)ds.Tables[0].Rows[1]["Department_SID"],
                        (string)ds.Tables[0].Rows[0]["Department"]
                    );
                    thread.Start(p2);
                }

                foreach (Thread thread in threads)
                {
                    thread.Join();
                }

                if (ErrorOnThread)
                    Assert.Fail("Dupe Test failed");
            }
        }
        void CreateDepartment(object p)
        {
            SQLDataLayer dl = null;
            MyParams par = (MyParams)p;
            try
            {
                dl = new SQLDataLayer(ResourceSettings.Instance.GetDBConnString());
                dl.BeginTransaction();
                DataSet ds = dl.ExecuteSP("dbo.sp_Department_Add",
                    dl.CreateParam("piDepartment", par.DeptValue),
                    dl.CreateParam("piIsIT", true),
                    dl.CreateParam("piActive", true));
                dl.Commit();
                if (ds.Tables[0].Rows[0]["Department_SID"] != DBNull.Value)
                    TestContext.WriteLine(string.Format("INSERTED for {0}", par.ThreadNum));                    
                else
                    TestContext.WriteLine(string.Format("Already exists for {0}", par.ThreadNum));
            }
            catch (Exception ex)
            {
                dl.Rollback();
                ErrorOnThread = true;
                TestContext.WriteLine(string.Format("Exception for {0}:{1}", par.ThreadNum, ex.Message));
            }
            finally
            {
                dl.Dispose();
            }
        }
        void UpdateDepartment(object p)
        {
            SQLDataLayer dl = null;
            UpdateDepartmentParams par = (UpdateDepartmentParams)p;
            try
            {
                dl = new SQLDataLayer(ResourceSettings.Instance.GetDBConnString());
                dl.BeginTransaction();
                DataSet ds = dl.ExecuteSP("dbo.sp_Department_Update",
                    dl.CreateParam("piDepartmentSid", par.DepartmentSid),
                    dl.CreateParam("piDepartment", par.DeptValue),
                    dl.CreateParam("piIsIT", true),
                    dl.CreateParam("piActive", true));
                dl.Commit();
                if ((int)ds.Tables[0].Rows[0]["RowsAffected"] > 0)
                    TestContext.WriteLine(string.Format("INSERTED for {0}{1}", par.DeptValue, par.ThreadNum));
                else
                    TestContext.WriteLine(string.Format("Already exists for {0}{1}", par.DeptValue, par.ThreadNum));
            }
            catch (Exception ex)
            {
                dl.Rollback();
                ErrorOnThread = true;
                TestContext.WriteLine(string.Format("Exception for {0}:{1}", par.ThreadNum, ex.Message));
            }
            finally
            {
                dl.Dispose();
            }
        }
        DataSet GetTopTwoDepartments()
        {
            DataSet ds = null;
            SQLDataLayer dl = null;
            try
            {
                dl = new SQLDataLayer(ResourceSettings.Instance.GetDBConnString());
                dl.BeginTransaction();
                ds = dl.ExecuteSQL("select top 2 Department_SID, Department from dbo.tb_Department;");
                dl.Commit();
                if (ds.Tables[0].Rows.Count <= 1)
                    throw new Exception("not enough departments to test with");
            }
            catch (Exception ex)
            {
                dl.Rollback();
                ErrorOnThread = true;
                TestContext.WriteLine(string.Format("Exception: ", ex.Message));
            }
            finally
            {
                dl.Dispose();
            }
            return ds;
        }
    }
}