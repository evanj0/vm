let iterations = 10;

const fib = n => {
    if (n <= 1) {
        return n;
    } else {
        return fib(n - 1) + fib(n - 2);
    }
};

startTime = performance.now();
for (let i = 0; i < iterations; i++) {
    fib(32);
}
endTime = performance.now();

time = endTime - startTime;
console.log(`${iterations} iterations in ${time} ms.`);
console.log(`Time in milliseconds: ${time / iterations}`);