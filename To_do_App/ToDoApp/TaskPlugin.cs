using System.ComponentModel;
using Microsoft.SemanticKernel;

public class TaskPlugin
{
    private readonly List<string> _tasks = new();

    [KernelFunction("add_task")]
    [Description("Adds a new task to the to-do list.")]
    public void AddTask([Description("The task description.")] string task)
    {
        _tasks.Add(task);
        Console.WriteLine($"📝 Task added: {task}");
    }

    [KernelFunction("show_tasks")]
    [Description("Returns a list of all tasks currently in the to-do list.")]
    public List<string> ShowTasks()
    {
        return _tasks.Count > 0 ? _tasks : new List<string> { "📌 No tasks found." };
    }

    [KernelFunction("complete_task")]
    [Description("Marks a task as completed and removes it from the list.")]
    public bool CompleteTask([Description("The task to complete.")] string task)
    {
        if (_tasks.Remove(task))
        {
            Console.WriteLine($"✅ Task completed: {task}");
            return true;
        }

        Console.WriteLine($"⚠️ Task not found: {task}");
        return false;
    }
}
