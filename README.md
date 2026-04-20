<p align="center">
  <h1 align="center">🧩 PuaSuiteClean</h1>
  <p align="center"><b>Binary Patch Injection & Analysis Framework</b></p>
  <p align="center">Clean • Deterministic • Research-Grade</p>
</p>

---

<p align="center">
  <img src="https://img.shields.io/badge/.NET-6%2B-512BD4?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=csharp" />
  <img src="https://img.shields.io/badge/Status-Experimental-orange?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Use-Research%20Only-red?style=for-the-badge" />
</p>


<img width="1079" height="737" alt="image" src="https://github.com/user-attachments/assets/ec770b55-7d2e-48c1-b42c-5f5d3331235c" />

---

## 📌 Overview

**PuaSuiteClean** is a modular **binary patching and analysis toolkit** written in **C# (.NET)**.  
This repository focuses on the **Patch Injector** — a low-level engine capable of applying precise **byte-level modifications** to binary files using structured JSON patch definitions.

Designed for **reverse-engineering research**, **binary instrumentation**, and **educational analysis** — **not piracy**.

---

## ✨ Key Features

- ✔ Byte-pattern based patching  
- ✔ Absolute offset patching  
- ✔ JSON-driven patch definitions  
- ✔ Supports single or multiple patches  
- ✔ Safe bounds checking  
- ✔ Deterministic behavior  
- ✔ Pure managed code (no unsafe blocks)

---

## 🧠 Core Component

PuaSuiteClean.Logic.PatchInjector

---

## 🔌 Public API

`bool ApplyPatch(string patchFile, string targetFile, out string message)`

**Parameters**

- `patchFile` — Path to JSON patch definition  
- `targetFile` — Target binary to modify  
- `message` — Execution log (output)

**Return Value**

- `true` — At least one patch applied  
- `false` — No patch applied or failure  

---

## 📂 Project Structure

PuaSuiteClean/  
└─ Logic/  
&nbsp;&nbsp;&nbsp;└─ PatchInjector.cs  

Minimal, modular, UI-agnostic by design.

---

## 🧩 Patch File Format

**Single Patch**

```json
{
  "Description": "Disable license check",
  "FindBytes": "74 05 E8",
  "ReplaceBytes": "90 90 90",
  "Offset": 0
}
```

**Multiple Patches**

```json
[
  {
    "Description": "Patch A",
    "FindBytes": "74 05",
    "ReplaceBytes": "90 90",
    "Offset": 0
  },
  {
    "Description": "Patch B",
    "ReplaceBytes": "90 90",
    "Offset": 123456
  }
]
```

---

## 📋 Patch Instruction Fields

| Field | Required | Notes |
|------|----------|------|
| Description | ❌ | Human-readable description |
| FindBytes | ⚠️ | Hex bytes, space-separated |
| ReplaceBytes | ✅ | Must match length |
| Offset | ❌ | Overrides pattern search if > 0 |

**Resolution Rules**

- Offset > 0 → Direct patch  
- Offset = 0 → Pattern scan  
- Invalid entries are skipped safely  

---

## 🔄 Execution Flow

Validate files →  
Load patch definitions →  
Read binary into memory →  
Resolve patch location →  
Bounds check →  
Apply patch →  
Write file if at least one patch succeeded  

---

## 🛡️ Safety & Error Handling

- Missing files → clean failure  
- Invalid hex → patch skipped  
- Pattern not found → patch skipped  
- Out-of-bounds write → prevented  
- JSON parse error → handled gracefully  

No partial corruption is written unless a valid patch is applied.

---

## ▶ Example Usage

```csharp
var injector = new PatchInjector();
bool success = injector.ApplyPatch(
    "patch.json",
    "target.exe",
    out string log
);
Console.WriteLine(log);
```

---

## ⚠ Legal & Ethical Notice

This project is intended for:

- Research  
- Education  
- Reverse-engineering practice  
- Binary instrumentation  

Not for:

- DRM circumvention  
- License bypassing  
- Unauthorized software modification  

You are responsible for ensuring lawful use.

---

## 🧭 Design Philosophy

Explicit > clever  
Deterministic > heuristic  
Safety > speed  

Clean tools beat clever hacks.

---

<p align="center">
  🛠️ <b>Research responsibly.</b>
</p>
