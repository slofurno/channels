#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>

int hello_open (char *path, int flags) {
    printf("path: %s\n", path);
    int fd = open(path, flags);

    if (fd < 0) {
        printf("open error: %s\n", strerror(errno));
    }

    return fd;
}

int hello_read (int fd, void *buffer, int count) {
    printf("start read, max count: %d\n", count);
    int n = read(fd, buffer, count);
    if (n < 0) {
        printf("read error: %s\n", strerror(errno));
    }
    printf("read %d\n", n);

    return n;
}

void last_error() {
    printf("%s\n", strerror(errno));
}

int addInts(int a, int b) {
    return a + b;
}

