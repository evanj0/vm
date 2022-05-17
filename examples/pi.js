const iterations = 10_000_000;

const pi = nMax => {
    let sum = 0;
    for (let n = 0; n < nMax; n++) {
        sum += 2.0 / ((4 * n + 1) * (4 * n + 3));
    }
    return 4 * sum;
};

let startTime = performance.now();
pi(iterations);
let endTime = performance.now();

let time = endTime - startTime;
console.log(`${iterations} iterations in ${time} ms.`);