#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <mqueue.h>
#include <sys/stat.h>
#include <unistd.h>
#include <string.h>
#include <errno.h>

char* mqueue_read(char *queue_name) {

    printf("reading queue %s\n", queue_name);
    struct mq_attr attr = {0};
    mqd_t queue = mq_open(queue_name, O_RDONLY);

    if (queue < 0) {
        printf("error: %s\n", strerror(errno));
    }

    mq_getattr(queue, &attr);
    char *msg = calloc(1, attr.mq_msgsize);

    ssize_t n;
    if ((n = mq_receive(queue, msg, attr.mq_msgsize, NULL)) == -1) {
        printf("error: %s\n", strerror(errno));
    } else {
        printf("mqueue_read returning: %s\n", msg);
    }

    mq_close(queue);
    return msg;
}

int mqueue_write(char *queue_name, char *msg) {
    size_t n = strlen(msg);
    mqd_t queue = mq_open(queue_name, O_RDWR | O_CREAT | O_NONBLOCK, 0664, NULL);
    mq_send(queue, msg, n, 0);
    mq_close(queue);
    return 0;
}

int open_file(char *name) {
    printf("try to open: %s\n", name);
    int fd = open(name, O_RDONLY|O_NONBLOCK);
    if (fd < 0) {
        printf("%s\n", strerror(errno));
    } else {
        printf("opened with fd: %d\n", fd);
    }
    return fd;
}


int main(int argc, char **argv){

    char *flag = argv[1];
    char *queue_name = argv[2];

    if (strcmp(flag, "-r") == 0) {
        mqueue_read(queue_name);
    } else if (strcmp(flag, "-w") == 0){
        mqueue_write(queue_name, argv[3]);
    }

    return 0;
}

