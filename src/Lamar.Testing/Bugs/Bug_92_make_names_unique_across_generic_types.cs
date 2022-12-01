using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_92_make_names_unique_across_generic_types
{
    [Fact]
    public void names_are_unique()
    {
        var container = Container.For(_ => { _.For(typeof(IRepository<>)).Use(typeof(Repository<>)); });

        container.GetInstance<UserService>()
            .ShouldNotBeNull();
    }

    public class UserService
    {
        private IRepository<UserAudit> userAuditRepo;
        private IRepository<UserDecision> userDecisionRepo;
        private IRepository<UserMessage> userMessageRepo;
        private IRepository<User> userRepo;

        public UserService(IRepository<UserMessage> userMessageRepo, IRepository<UserDecision> userDecisionRepo,
            IRepository<UserAudit> userAuditRepo, IRepository<User> userRepo)
        {
            this.userMessageRepo = userMessageRepo;
            this.userDecisionRepo = userDecisionRepo;
            this.userAuditRepo = userAuditRepo;
            this.userRepo = userRepo;
        }


        public void FindByUsername(string username)
        {
        }
    }

    public interface IRepository<T>
    {
    }

    public class Repository<T> : IRepository<T>
    {
    }

    public class User
    {
    }

    public class UserAudit
    {
    }

    public class UserDecision
    {
    }

    public class UserMessage
    {
    }
}