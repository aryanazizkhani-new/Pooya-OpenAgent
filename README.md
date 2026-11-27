# OpenAgent 🤖

**OpenAgent** is an open-source project focused on bringing the classic **Microsoft Agent** experience back to modern Windows.

The goal is to make classic animated assistants such as **Bonzi, Peedy, Merlin, and Rover** usable again through a modern runtime and development environment.

> 🕰️ Bringing the Microsoft Agent experience back to modern Windows.

---

## ✨ Features

* 🤖 Microsoft Agent-style character support
* 💬 Character speech and text bubbles
* 🗣️ Text-to-Speech support and future integrations
* 🧩 Double Agent COM integration
* 🪟 Modern Windows compatibility
* 🔌 .NET-based runtime architecture
* 🎭 Support for custom characters
* 🛠️ Designed for developers and Agent-based applications

---

## 🧠 Architecture

OpenAgent is divided into several components:

```text
OpenAgent
│
├── OpenAgent.Interop
│   └── COM interfaces / Double Agent interop
│
├── OpenAgent.Runtime
│   └── Agent runtime and character management
│
├── OpenAgent.Sample
│   └── Example application
│
└── OpenAgent.slnx
    └── Main solution
```

### OpenAgent.Interop

This component provides the interoperability layer between OpenAgent and the Double Agent COM API.

```text
OpenAgent
    │
    ▼
OpenAgent.Interop
    │
    ▼
Double Agent COM
    │
    ▼
Agent Character
```

---

## 🤖 Double Agent

OpenAgent uses **Double Agent** as part of its compatibility layer for the classic Microsoft Agent ecosystem.

A typical connection looks like:

```text
✔ Double Agent COM Connected
✔ IDaServer Ready
✔ Agent Loaded
```

This allows OpenAgent to communicate with the Agent server and control characters.

---

## 🎭 Characters

OpenAgent is designed to support classic Microsoft Agent characters, including:

* Bonzi
* Peedy
* Merlin
* Genie
* Rover
* Custom characters

Characters can provide functionality such as:

```text
Show()
Hide()
Move()
Speak()
Think()
Animate()
```

---

## 🗨️ Speech

OpenAgent supports character messages through speech bubbles.

Example:

```text
┌─────────────────────────────┐
│ Hello! I'm OpenAgent! 🤖    │
└─────────────────────────────┘
              │
              ▼
           [Agent]
```

Future versions may provide support for multiple Text-to-Speech engines.

---
---

## 🤖 AI Providers

OpenAgent supports AI integration through multiple providers, allowing classic Agent characters to interact with modern AI models.

### Supported Providers

- OpenRouter
- Ollama (Local AI)

### Planned Providers

- OpenAI
- Google Gemini
- Anthropic Claude
- DeepSeek
- Mistral AI
- Groq
- xAI (Grok)
- Azure OpenAI

### Features

- AI-powered character conversations
- Multiple AI provider support
- Model selection
- Provider switching
- Conversation history
- Local AI support
- Custom API configuration

AI integration is designed to be modular, allowing new providers to be integrated without changing the core Agent runtime.

---

## 🛠️ Requirements

To build and develop OpenAgent, you will need:

* Windows
* .NET
* Visual Studio or VS Code
* Double Agent
* Required Double Agent COM/Type Library components

---

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-username/OpenAgent.git
cd OpenAgent
```

### 2. Build the project

Using the .NET CLI:

```bash
dotnet build
```

Or open the solution in Visual Studio.

### 3. Run the sample

```bash
dotnet run --project OpenAgent.Sample
```

---

## 🔌 COM Connection

When OpenAgent starts, it attempts to connect to the Double Agent server.

Example output:

```text
==================================================
                   OPENAGENT
==================================================

Connecting to Double Agent...

✔ Double Agent COM Connected
✔ IDaServer Ready

Loading character...

✔ Character Loaded

OpenAgent is ready.
```

---

## 🧪 Example

A basic OpenAgent application could look like:

```csharp
var agent = new OpenAgentRuntime();

agent.Connect();

var character = agent.LoadCharacter("Bonzi");

character.Show();

character.Speak("Hello world!");
```

> The API is currently under development and may change between versions.

---

## 📁 Project Structure

```text
OpenAgent/
│
├── OpenAgent.slnx
│
├── OpenAgent.Interop/
│   ├── DaServer.tlb
│   └── ...
│
├── OpenAgent.Runtime/
│   ├── AgentRuntime.cs
│   ├── Character.cs
│   └── ...
│
├── OpenAgent.Sample/
│   ├── Program.cs
│   └── ...
│
├── README.md
└── LICENSE
```

---

## 🎯 Roadmap

### Core

* [x] Double Agent connection
* [x] `IDaServer` access
* [x] Character loading
* [x] Character display
* [x] Text messages
* [ ] Full animation system
* [ ] Event system
* [ ] Multiple character management

### Speech

* [ ] Text-to-Speech
* [ ] Improved multilingual support
* [ ] Voice selection
* [ ] Speech speed control
* [ ] Voice pitch control

### Characters

* [ ] Bonzi
* [ ] Peedy
* [ ] Merlin
* [ ] Genie
* [ ] Rover
* [ ] Custom character support

### Modern UI

* [ ] Improved Windows 11 support
* [ ] Modern settings
* [ ] Character manager
* [ ] Voice manager
* [ ] Animation preview

---

## 🕰️ Why OpenAgent?

Microsoft Agent was a unique part of Windows history.

Characters such as **Merlin, Peedy, and Rover** gave computers a more interactive and personal experience.

OpenAgent aims to bring that concept back:

```text
Microsoft Agent
       │
       ▼
  Double Agent
       │
       ▼
    OpenAgent
       │
       ▼
 Modern Windows
```

The goal isn't simply to run old software.

The goal is to create a modern runtime for the classic Agent experience.

---

## ⚠️ Project Status

OpenAgent is currently **under active development** and should be considered experimental.

Some features may not be fully implemented, and the API may change as the project evolves.

If something does not work, make sure that:

1. Double Agent is correctly installed.
2. The required COM components are available.
3. The requested character is installed.
4. The correct .NET version is being used.

---

## 🤝 Contributing

Contributions are welcome!

Useful areas for contribution include:

* New characters
* New animations
* Text-to-Speech integrations
* Multilingual support
* COM interop improvements
* UI development
* Bug fixes
* Runtime improvements

---

## 📜 License

OpenAgent is an independent, unofficial project.

**Microsoft Agent, Double Agent, and related character names and assets belong to their respective rights holders.**

OpenAgent is not affiliated with or endorsed by Microsoft.

---

## 💙 OpenAgent

> **Old assistants. New Windows.**
> **Bringing classic Agents back to life.** 🤖
