using System.Collections.Generic;

public class ChatHistory
{
    public List<ChatMessage> Messages { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; }
    public string Content { get; set; }
}