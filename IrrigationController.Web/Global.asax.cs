using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace IrrigationController.Web
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            (new AppHost()).Init();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //Exception objErr = Server.GetLastError().GetBaseException();
            //Server.ClearError();

            //if (objErr.GetType() == typeof(System.Reflection.ReflectionTypeLoadException))
            //{
            //    System.Reflection.ReflectionTypeLoadException reflErr = (System.Reflection.ReflectionTypeLoadException)objErr;
            //    foreach (Exception ex in reflErr.LoaderExceptions)
            //        sb.AppendLine(ex.Message);
            //}

            //sb.AppendLine("Error Message: " + objErr.Message.ToString());
            //sb.AppendLine("Stack Trace: " + objErr.StackTrace.ToString());
            //Response.Write(sb.ToString());
            //Response.End();
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}