from time import perf_counter_ns


iterations = 10_000_000

def pi(n_max):
    sum = 0
    for n in range(n_max):
        sum += 2.0 / ((4 * n + 1) * (4 * n + 3))
    return 4 * sum

if __name__ == "__main__":
    start_time = perf_counter_ns()
    pi(iterations)
    end_time = perf_counter_ns()
    time = (end_time - start_time) / 1_000_000 # ns to ms
    print(f"{iterations} iterations in {time} ms.")