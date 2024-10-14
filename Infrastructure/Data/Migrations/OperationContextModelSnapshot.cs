﻿// <auto-generated />
using System;
using AggregateVersions.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AggregateVersions.Infrastructure.Migrations
{
    [DbContext(typeof(OperationContext))]
    partial class OperationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AggregateVersions.Domain.Entities.Access", b =>
                {
                    b.Property<long?>("ApplicationId")
                        .HasColumnType("bigint");

                    b.Property<int>("CommonnessStatus")
                        .HasColumnType("int");

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DisplayCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("Guid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("ID")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsApi")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsEnable")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSharedInSubsystems")
                        .HasColumnType("bit");

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<long?>("ModifiedBy")
                        .HasColumnType("bigint");

                    b.Property<long?>("ParentId")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("TypeId")
                        .HasColumnType("bigint");

                    b.ToTable("COM_ACC_Access", (string)null);
                });

            modelBuilder.Entity("AggregateVersions.Domain.Entities.DataBase", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProjectID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ID");

                    b.HasIndex("ProjectID");

                    b.ToTable("DataBases");
                });

            modelBuilder.Entity("AggregateVersions.Domain.Entities.Project", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("AggregateVersions.Domain.Entities.DataBase", b =>
                {
                    b.HasOne("AggregateVersions.Domain.Entities.Project", "Project")
                        .WithMany("DataBases")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("AggregateVersions.Domain.Entities.Project", b =>
                {
                    b.Navigation("DataBases");
                });
#pragma warning restore 612, 618
        }
    }
}
