# fibonacci sequence

```
iterations = 10
fib(32)
```

```
asm ./fib.txt -o ./out/fib
vm ./out/fib

python ./fib.py
node ./fib.js
vm benchmark ./out/fib --iterations 10
```

# leibnitz formula

```
pi(10_000_000)
pi = 3.14159265359
```
```
asm ./pi.txt -o ./out/pi
vm ./out/pi

python ./pi.py
node ./pi.js
vm benchmark ./out/pi --iterations 1
```
