using System.Collections.Generic;

namespace HTNAI {
    public class Plan {
        public List<PrimitiveTask> Tasks { get; private set; }

        public Plan() {
            Tasks = new List<PrimitiveTask>();
        }

        public Plan(Plan plan) {
            Tasks = plan.Tasks;
        }

    }
}