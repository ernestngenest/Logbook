﻿// <auto-generated />
using System;
using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kalbe.App.InternshipLogbookLogbook.Api.Migrations
{
    [DbContext(typeof(InternshipLogbookLogbookDataContext))]
    [Migration("20240119150246_addYearLogbook")]
    partial class addYearLogbook
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.20")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "citext");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Kalbe.App.InternshipLogbookLogbook.Api.Models.ActivityLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Activity")
                        .HasColumnType("citext");

                    b.Property<string>("AppCode")
                        .HasColumnType("citext");

                    b.Property<string>("CompanyId")
                        .HasColumnType("citext");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("citext");

                    b.Property<string>("CreatedByName")
                        .HasColumnType("citext");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("DocumentNumber")
                        .HasColumnType("citext");

                    b.Property<string>("ExternalEntity")
                        .HasColumnType("citext");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("LogType")
                        .HasColumnType("citext");

                    b.Property<string>("Message")
                        .HasColumnType("citext");

                    b.Property<string>("ModuleCode")
                        .HasColumnType("citext");

                    b.Property<string>("PayLoad")
                        .HasColumnType("citext");

                    b.Property<string>("PayLoadType")
                        .HasColumnType("citext");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("citext");

                    b.Property<string>("UpdatedByName")
                        .HasColumnType("citext");

                    b.Property<DateTime?>("UpdatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("t_Logger");
                });

            modelBuilder.Entity("Kalbe.App.InternshipLogbookLogbook.Api.Models.InternshipLogbookLogbook", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("Allowance")
                        .HasColumnType("bigint");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("citext");

                    b.Property<string>("CreatedByName")
                        .HasColumnType("citext");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("DepartmentName")
                        .IsRequired()
                        .HasColumnType("citext");

                    b.Property<string>("DocNo")
                        .HasColumnType("citext");

                    b.Property<string>("FacultyCode")
                        .HasColumnType("citext");

                    b.Property<string>("FacultyName")
                        .IsRequired()
                        .HasColumnType("citext");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Month")
                        .IsRequired()
                        .HasColumnType("citext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("citext");

                    b.Property<string>("SchoolCode")
                        .HasColumnType("citext");

                    b.Property<string>("SchoolName")
                        .IsRequired()
                        .HasColumnType("citext");

                    b.Property<string>("Status")
                        .HasColumnType("citext");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("citext");

                    b.Property<string>("UpdatedByName")
                        .HasColumnType("citext");

                    b.Property<DateTime?>("UpdatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Upn")
                        .HasColumnType("citext");

                    b.Property<int>("WFHCount")
                        .HasColumnType("integer");

                    b.Property<int>("WFOCount")
                        .HasColumnType("integer");

                    b.Property<string>("Year")
                        .HasColumnType("citext");

                    b.HasKey("Id");

                    b.ToTable("t_Logbook");
                });

            modelBuilder.Entity("Kalbe.App.InternshipLogbookLogbook.Api.Models.LogbookDays", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Activity")
                        .IsRequired()
                        .HasColumnType("citext");

                    b.Property<long>("AllowanceFee")
                        .HasColumnType("bigint");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("citext");

                    b.Property<string>("CreatedByName")
                        .HasColumnType("citext");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<long>("LogbookId")
                        .HasColumnType("bigint");

                    b.Property<string>("Status")
                        .HasColumnType("citext");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("citext");

                    b.Property<string>("UpdatedByName")
                        .HasColumnType("citext");

                    b.Property<DateTime?>("UpdatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("WorkType")
                        .IsRequired()
                        .HasColumnType("citext");

                    b.HasKey("Id");

                    b.HasIndex("LogbookId");

                    b.ToTable("d_LogbookDays");
                });

            modelBuilder.Entity("Kalbe.App.InternshipLogbookLogbook.Api.Models.LogbookDays", b =>
                {
                    b.HasOne("Kalbe.App.InternshipLogbookLogbook.Api.Models.InternshipLogbookLogbook", "Logbook")
                        .WithMany("LogbookDays")
                        .HasForeignKey("LogbookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Logbook");
                });

            modelBuilder.Entity("Kalbe.App.InternshipLogbookLogbook.Api.Models.InternshipLogbookLogbook", b =>
                {
                    b.Navigation("LogbookDays");
                });
#pragma warning restore 612, 618
        }
    }
}
