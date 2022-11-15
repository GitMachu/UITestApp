# UITestApp
A Selenium UI script running tool

A tool to run scripts which contain steps that the tool will follow in order to fulfill automation tasks.

Scripts are in XML format and are displayed upon opening the app. 5 scripts have been created that correspond to each UI test case.

The tool serves as a script creator and runner. Upon clicking either run option, the tool will setup an automation driver, spawn the chosen browser, then run automation tests.

The Run Single button pertains to one thread. Any script runs initiated by the Run Single button only runs on that thread, and cannot do so if there's a test running.

The Run Parallel button looks at 5 threads and executes the test on the first available thread it sees. If all 5 threads are busy, the test won't execute.

Scripts can run on either the Chrome, Firefox, or Edge browser.

A status logs window will appear when running, which allows users to see what the tool is currently doing.

Execution on a thread will stop immediately if a step fails, or if the user cancels the run by clicking on the corresponding thread's status logs window.

A UI step can fail due to a variety of reasons, like incorrect data, slow loading of a page, or unexpected behavior, among other things.

After a script run, a results window will appear, showing users whether a step passed, failed, or got skipped. Users can also look at the status logs during and after execution for more details.
