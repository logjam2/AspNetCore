# LogJam.AspNetCore TODO

1. Read LogJamLoggingOptions from JSON + XML config
  * Support IncludeScopes: false
1. Make proxy/fanout EntryWriters self-updating when downstream entrywriters are started or stopped
  * Test that restart with config changes works correctly when using a single ILogger
2. Try hierarchical configuration - eg different LogLevel for Debug, Console, and File
3. Memory profiling
4. Performance profiling
5. Http Server logging
6. Http Client logging
7. EF SQL logging
