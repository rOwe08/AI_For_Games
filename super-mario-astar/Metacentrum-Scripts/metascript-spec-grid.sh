#!/bin/bash

# seq -> FIRST INCREMENT LAST

for ndw in 0.00 0.50 1.00 1.50 2.00 3.00
do
  for ttfw in 0.00 0.50 1.00 1.50 2.00
  do
    for dfpt in 0.00 1.00 2.00 3.00 4.00 5.00 7.00 10.00 15.00 20.00
    do
      for dfpap in 0.00 1.00 2.00 3.00 5.00 10.00 20.00 50.00
      do
          sed -i "s/NDW=[0-9]\+\.[0-9]\+/NDW=$ndw/" script-spec-grid.sh;
          sed -i "s/TTFW=[0-9]\+\.[0-9]\+/TTFW=$ttfw/" script-spec-grid.sh;
          sed -i "s/DFPT=[0-9]\+\.[0-9]\+/DFPT=$dfpt/" script-spec-grid.sh;
          sed -i "s/DFPAP=[0-9]\+\.[0-9]\+/DFPAP=$dfpap/" script-spec-grid.sh;
          #qsub script-spec-grid.sh;
          cat script-spec-grid.sh;
      done
    done
  done
done
