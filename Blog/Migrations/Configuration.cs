namespace Blog.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using System.IO;

    public sealed class Configuration : DbMigrationsConfiguration<Blog.Models.BlogDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(Blog.Models.BlogDBContext context)
        {
            if (!context.Roles.Any())
            {
                this.CreateRole(context, "Admin");
                this.CreateRole(context, "User");
            }

            if (!context.Users.Any())
            {
                this.CreateUser(context, "admin@admin.com", "Admin", "123");
                this.SetRoleToUser(context, "admin@admin.com", "Admin");

                var user = context.Users
                    .Where(u => u.Email.Equals("admin@admin.com"))
                    .First();

                var username = user.UserName;
                var fnm = username + ".png";
                var filePath = HostingEnvironment.MapPath("~/Content/images/profile/") + fnm;
                var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/Content/images/profile/"));
                var startPath = HostingEnvironment.MapPath("~/Content/empty.png");
                if (directory.Exists == false)
                {
                    directory.Create();
                }

                File.Copy(startPath, filePath, true);
            }
        }

        private void SetRoleToUser(BlogDBContext context, string email, string role)
        {
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            var user = context.Users.Where(u => u.Email == email).First();

            var result = userManager.AddToRole(user.Id, role);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }
        }

        private void CreateUser(BlogDBContext context, string email, string fullName, string password)
        {
            // create user manager
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            // set user manager password validator
            userManager.PasswordValidator = new PasswordValidator()
            {
                RequiredLength = 1,
                RequireDigit = false,
                RequireLowercase = false,
                RequireNonLetterOrDigit = false,
                RequireUppercase = false,
            };

            // create user object
            var admin = new ApplicationUser
            {
                UserName = email,
                FullName = fullName,
                Email = email,
            };

            // create user
            var result = userManager.Create(admin, password);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }
        }

        private void CreateRole(BlogDBContext context, string roleName)
        {
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            var result = roleManager.Create(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }
        }
    }
}
