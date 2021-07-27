# KTDReader

## Introduction

This utility will read all of the records in an 'F3' data file as used by the Fiat ePer database.  These files are used to hold records of 
car production and so are a source of chassy and engine numbers as well as information about the
exact model of car.  The records only contain key information such as model reference (SINCOM)
that can then be turned into text using the tables in the ePer database.

This utility just dumps out all records to a flat, tab-delimited file that can then be loaded into SQL or similar.  The resulting files are very large so will take time
to load into a viewer.







