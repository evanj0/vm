from time import perf_counter, perf_counter_ns

iterations = 10

def fib(n):
    if  n <= 1:
        return n
    else:
        return fib(n - 1) + fib(n - 2)

if __name__ == "__main__":
    start_time = perf_counter_ns()
    for i in range(iterations):
        fib(32)
    end_time = perf_counter_ns()
    time = (end_time - start_time) / 1_000_000 # ns to ms
    print(f"{iterations} iterations in {time} ms.")
    print(f"Time in milliseconds: {time / iterations}")