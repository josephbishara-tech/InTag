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
    public enum PlatformModule
    {
        Asset,
        Document,
        Manufacturing,
        Maintenance,
        Inventory,
        Workflow
    }
}
