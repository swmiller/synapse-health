# Copilot Instructions: Signal Booster Assignment

## Scenario

You have inherited a utility that processes physician notes to extract information about patient durable medical equipment (DME) needs (e.g., CPAPs, oxygen tanks) and sends structured data to an external API. The original code is minimalistic, with all logic in `Main`, cryptic variable names, misleading comments, unused logic, and lacking logging, error handling, and unit tests. Your task is to refactor and improve this tool for reliability and maintainability.

## Mission

1. Refactor logic into well-named, testable methods:

   - Improve structure and readability
   - Remove redundant or dead code
   - Use clear and consistent naming

2. Introduce logging and basic error handling:

   - Avoid swallowing exceptions
   - Log meaningful steps for observability

3. Write at least one unit test:

   - Demonstrate testing of a meaningful part of the logic

4. Replace misleading or unclear comments with helpful ones

5. Maintain functionality:

   - Read a physician note from a file
   - Extract structured data (device type, provider, etc.)
   - POST the data to `https://alert-api.com/DrExtract` (not a real link)

6. (Optional stretch goals):
   - Use an LLM (OpenAI/Azure OpenAI) for extraction
   - Accept multiple input formats (e.g., JSON-wrapped notes)
   - Add configurability for file path or API endpoint
   - Support more DME device types or qualifiers

## README Requirements

Include a short README with:

- IDE or tools used (e.g., VS Code, Rider, Visual Studio)
- Any AI development tools used (e.g., GitHub Copilot, Cursor, Cody)
- Assumptions, limitations, or future improvements
- Instructions to run the project (if needed)

## AI Tool Usage

You are encouraged to use AI tools to complete this assignment. Integration of modern development practices is part of the evaluation.

## Language Flexibility

If you are not a C# developer, you may rewrite the solution in your preferred language and follow the above instructions.
