using System;
using System.Collections.Generic;
using System.Text;

namespace InTagEntitiesLayer.Enums
{
    /// <summary>
    /// RBAC roles per module: Admin, Manager, Operator, Viewer
    /// </summary>
    public enum ModuleRole
    {
        Viewer = 0,
        Operator = 1,
        Manager = 2,
        Admin = 3
    }

    /// <summary>
    /// Platform modules for per-module RBAC
    /// </summary>
    //public enum PlatformModule
    //{
    //    Asset,
    //    Document,
    //    Manufacturing,
    //    Maintenance,
    //    Inventory,
    //    Workflow
    //}

    public enum PlatformModule
    {
        Asset = 0,
        Document = 1,
        Manufacturing = 2,
        Maintenance = 3,
        Inventory = 4,
        Workflow = 5,
        Sales = 6,
        Purchase = 7,
        Accounting = 8
    }
}
