using InTagLogicLayer.Workflow;

namespace InTagWeb.Configuration
{
    public static class WorkflowServiceRegistration
    {
        public static IServiceCollection AddInTagWorkflowServices(
            this IServiceCollection services)
        {
            services.AddScoped<IWorkflowService, WorkflowService>();
            services.AddScoped<INotificationService, NotificationService>();
            return services;
        }
    }
}
