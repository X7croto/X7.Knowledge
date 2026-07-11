using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge.Execution.Builders;

public sealed class ExecutionKnowledgeBuilder
{
    public ExecutionFlow Build(ProjectIndexOld index)
    {
        var execution = new ExecutionFlow();

        //new RequestFlowBuilder()
        //    .Build(index, execution);

        //new PipelineBuilder()
        //    .Build(index, execution);

        //new EventFlowBuilder()
        //    .Build(index, execution);

        //new TransactionBuilder()
        //    .Build(index, execution);

        //new BackgroundServiceBuilder()
        //    .Build(index, execution);

        //new DIBuilder()
        //    .Build(index, execution);

        return execution;
    }
}