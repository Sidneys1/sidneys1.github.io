---
layout: post
title:  "Performant .NET, Part 1: Resource Contention"
categories:
    - .NET
    - Performance
---
# Performance Please?

.NET, and particularly C#, are wonderful development tools for writing quick-and-dirty applications that just get the job done.

Sometimes, however, your solution expands beyond your original design considerations, and requires redesign and tweaking to handle the new load.

# What is Resource Contention?

Resource contention occurs when two or more threads attempt to access a programmatic resource (such as a variable, property, etc) at the same time.
This can cause data corruption, so a way to control when which thread gets access to the resource must be implemented.

A common tool is the [lock](https://msdn.microsoft.com/en-us/library/c5kehkcz.aspx) keyword, which provides threads access to a resource on a first-come-first-serve basis.
Threads accessing a locked resource will be put on hold until the active thread releases its lock.

{% highlight csharp %}
// Only reference types can be locked.
// To lock a value type, you must create a 'dummy' reference type
int Resource = 0;
object ResourceLock = new object();
...
// In a thread somewhere...
lock (ResourceLock) {
    Resource++;
}
{% endhighlight %}

# What Can I Do?

Avoid using `lock` at all costs.
It's slow, clunky, and just a bad solution.

## Interlocked

For basic value types, like `int` and `float`, you can use the [Interlocked](https://msdn.microsoft.com/en-us/library/system.threading.interlocked(v=vs.110).aspx) class.
It contains methods that will perform atomic operations on the resource.
(An atomic operation being one that happens in a single CPU cycle, meaning no other threads can possibly modify or read the value before the operation is complete)

## Concurrent Collections

# Example

I've created [an example .NET project](https://github.com/Sidneys1/sidneys1.github.io/tree/examples/Performant%20.NET/ResourceContention) to demonstrate the potential benefits of Concurrent Collections.

Using [SysInternals' Process Explorer](https://technet.microsoft.com/en-us/sysinternals/processexplorer.aspx) we can monitor the number a times resources have been contested in a program.

Running the example program, we can see that the first class used ([Conflicts.cs](https://github.com/Sidneys1/sidneys1.github.io/blob/examples/Performant%20.NET/ResourceContention/Conflicts.cs)) executes in just over 1 second.

```
Using 8 threads...
Walking....
Done! Found 191,900 files!
Press enter to start...
Done in 1,153ms!
```
Looking at Process Explorer, we can see that the number of contentions is quite high.

![resource_contention_pt1_run1]({{ site.baseurl }}/uploads\resource_contention_pt1_run1.png){: .center-image }

The second run uses a class that takes advantage of concurrent collections ([NoConflicts.cs](https://github.com/Sidneys1/sidneys1.github.io/blob/examples/Performant%20.NET/ResourceContention/NoConflicts.cs))

```
Walking....
Done! Found 191,900 files!
Press enter to start...
Done in 475ms!
```

As you can see, not only is the second run faster, but there have been no additional resource contentions!

![resource_contention_pt1_run2]({{ site.baseurl }}/uploads\resource_contention_pt1_run2.png){: .center-image }