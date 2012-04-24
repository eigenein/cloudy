#include <stdio.h>
#include <math.h>
#include <mpi.h>

int main (int argc, char *argv[])
{ 
    int n, myid, numprocs, i;
    double mypi, pi, h, sum, x, t1, t2;

    MPI_Init(&argc, &argv);
    MPI_Comm_size(MPI_COMM_WORLD, &numprocs);
    MPI_Comm_rank(MPI_COMM_WORLD, &myid);
    if (myid == 0)
    {
        if (argc < 1) 
        {
            return;
        }
        sscanf(argv[1], "%d", &n);
        t1 = MPI_Wtime();
    }
    MPI_Bcast(&n, 1, MPI_INT, 0, MPI_COMM_WORLD);
    h = 1.0 / (double)n;
    sum = 0.0;
    for (i = myid + 1; i <= n; i += numprocs)
    {
        x = h * ((double)i - 0.5);
        sum += (4.0 / (1.0 + x * x));
    }
    mypi = h * sum;
    MPI_Reduce(&mypi, &pi, 1, MPI_DOUBLE, MPI_SUM, 0, MPI_COMM_WORLD);
    if (myid == 0)
    { 
        t2 = MPI_Wtime();
        printf("Pi: %.16f\n", pi);
        printf("Time: %fs\n", t2 - t1);
    }
    MPI_Finalize();
    return 0;
}
