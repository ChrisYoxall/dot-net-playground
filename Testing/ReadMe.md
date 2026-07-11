## Unit Tests vs Integration Tests ##

Tools such as `WebApplicationFactory` and Test Containers make integration tests almost as easy as
writing unit-tests. When would you use each?

Integration Tests:
- Still usually a bit more heavyweight and slow to run.
- Use to prove the feature works end to end: routing, validation, persistence.
- Verify the happy path, and the important sad paths.

Unit Tests:
- Hammer the interesting domain logic with every edge case, at nanosecond speed.
- If a component has no interesting logic, it gets no unit tests.
