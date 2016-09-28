CFLAGS=-Wall -Wextra -std=c99

all: install

mqueue: mqueue.c
	gcc $(CFLAGS) mqueue.c -lrt -o mqueue

libhello.so: hello.c
	gcc $(CFLAGS) -shared -fPIC hello.c -o libhello.so

libmq.so: mqueue.c
	gcc $(CFLAGS) -shared -fPIC mqueue.c -o libmq.so

install: libhello.so libmq.so
	mv libhello.so ./bin/Debug/netcoreapp1.0
	mv libmq.so ./bin/Debug/netcoreapp1.0
