

# IBM Decision Optimization Modeling with OPL Custom data source in .net sample

This sample demonstrates how to use the IBM Decision Optimization Modeling with
OPL custom data source .net API to import data from a data source into an OPL model.

This sample illustrates the [Working with OPL interfaces -> Custom data sources](https://www.ibm.com/support/knowledgecenter/SSSA5P_12.7.0/ilog.odms.ide.help/OPL_Studio/usroplinterfaces/topics/opl_interfaces_work_custom.html) section from the OPL User's manual.
This sample shows you how to read and write tuplesets to/from a database with .net. It also enables you to read a database and [generate .dat files](#export_dat_files) to be used in the IDE to prototype your optimization model.

While this sample uses Microsoft SQL Server as data storage, it can be easily adapted to
any database that provide a .net client.
This example will work with any 12.x OPL version, even if it is configured to run with 12.7.0 version.


## Prerequisites

1. This sample assumes that IBM ILOG CPLEX Optimization Studio 12.7.0 is
   installed and configured in your environment.

2. This sample assumes you have Visual Studio 2013 installed.   
   
3. <em>Optional:</em> To run the sample with SQL Server, download and install [Microsoft SQL Server 2014 Express](https://www.microsoft.com/fr-fr/server-cloud/products/sql-server-editions/sql-server-express.aspx).

4. <em>Optional:</em> To run the sample with Access, download and install the [Microsoft Access Data Engine 2010 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=13255) if you don't have Access installed
on your machine.


## Build the sample

The solution `CustomDataSourceSample.sln` is configured to use the OPL .NET API that
is in `$(CPLEX_STUDIO_DIR127)\opl\lib\oplall.dll`. The `CPLEX_STUDIO_DIR127` environment
variable is set by the IBM CPLEX Studio installer.

In case it is not set, you need to add the OPL .NET APIs.

1. In the Solution view, select the project (CustomDataSourceSample), the References, and click <b>Add Reference</b>.

2. In the Add Reference Wizard, select Browse, navigate to the OPL installation, and add <OPL>/lib/oplall.dll.

Build the project with <b>BUILD</b> -> <b>Build Solution</b> (Ctrl+Shift+B).

## Run the sample with <b>Microsoft SQL Server</b>

Edit `data\db_mssql.xml` for your database connection string.

Launch the sample using the Visual Studio <b>Debug/Start Debugging</b> command.

The default Start solution configuration appends `-export model.dat -create_sample -configuration ../../data/db_mssql.xml` to the command line. This has the effect of:

* use the `data/db_mssql.xml` configuration, which is the default configuration for SQL Server.
* create sample demo database on program startup.
* export data to file `result.dat`.

## Run the sample with <b>Microsoft Access</b>

Edit `data\db_access.xml` for your database connection string.

Left click `CustomDataSourceSample` in the <b>Solution Explorer</b>, then <b>properties</b>.
In the properties window, navigate to the <b>Debug</b> tab. In the command line arguments,
input: `-export model.dat -create_sample -configuration ..\..\data\db_access.xml`

Launch the sample using the Visual Studio <b>Debug/Start Debugging</b> command.
This will:

* use the `data/db_access.xml` configuration, which is the default configuration for Access using OleDb.
* create sample demo database on program startup.
* export data to file `result.dat`.
* with the default configuration, data are written to file `data\oil.accdb`.

## Export .dat files for use in OPL IDE
<a name="export_dat_files"></a>

* When running the `CustomDataSourceSample.exe` command, simply add `-export model.dat` on the command line, and it will export all the tuplesets that have been extracted from the database to `model.dat` file.
* The default run configuration for CustomDataSourceSample include `-export model.dat`, so after running the sample, you should have a `model.dat` file in `bin\Debug`

## Run with an OPL version <= 12.6.x
* In the `References` of the project, remove `oplall` then add a new reference with your 12.x version.
* Recompile the project

## License

This sample is delivered under the Apache License Version 2.0, January 2004 (see LICENSE.txt).
