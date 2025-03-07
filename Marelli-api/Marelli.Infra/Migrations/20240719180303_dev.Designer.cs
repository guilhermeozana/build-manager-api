﻿// <auto-generated />
using System;
using Marelli.Infra.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Marelli.Infra.Migrations
{
    [DbContext(typeof(DemurrageContext))]
    [Migration("20240719180303_local")]
    partial class local
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Marelli.Domain.Entities.BuildTableRow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Developer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool?>("Disabled")
                        .HasColumnType("boolean");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Md5Hash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("BuildTableRow");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.BuildingState", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BuildingKitfile")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("BuildingKitfileDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Compiling")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CompilingDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Download")
                        .HasColumnType("boolean");

                    b.Property<bool>("Finished")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("FinishedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Integrating")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("IntegratingDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("JenkinsBuildLogFile")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Linking")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LinkingDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("Starting")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("StartingDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("BuildingState");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Day", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("FailureValue")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("MonthId")
                        .HasColumnType("integer");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<decimal>("SuccessValue")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("MonthId");

                    b.ToTable("Day");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.FileVerify", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FileZipS3BucketLocation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsFileOk")
                        .HasColumnType("boolean");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("FileVerify");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id");

                    b.ToTable("Group");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.LogAndArtifact", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FileLogS3BucketLocation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FileOutS3BucketLocation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("LogAndArtifact");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.MainData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("MainData");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Month", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("Month");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.MonthData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Day")
                        .HasColumnType("integer");

                    b.Property<int>("FailValue")
                        .HasColumnType("integer");

                    b.Property<int>("MainId")
                        .HasColumnType("integer");

                    b.Property<int>("SucceedValue")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MainId");

                    b.ToTable("MonthData");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.News", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("News");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("Id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Name");

                    b.HasKey("Id");

                    b.ToTable("Product");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<string>("Image")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("Project");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<bool?>("NewUser")
                        .HasColumnType("boolean");

                    b.Property<string>("Password")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.UserGroup", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.HasKey("UserId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("UserGroup");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.UserProject", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.HasKey("UserId", "ProjectId");

                    b.HasIndex("ProjectId");

                    b.ToTable("UserProject");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Week", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("FailureValue")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("MonthId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<decimal>("SuccessValue")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("MonthId");

                    b.ToTable("Week");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.WeekData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Day")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)");

                    b.Property<int>("FailValue")
                        .HasColumnType("integer");

                    b.Property<int>("MainId")
                        .HasColumnType("integer");

                    b.Property<int>("SucceedValue")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MainId");

                    b.ToTable("WeekData");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Day", b =>
                {
                    b.HasOne("Marelli.Domain.Entities.Month", "Month")
                        .WithMany("Days")
                        .HasForeignKey("MonthId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Month");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.MonthData", b =>
                {
                    b.HasOne("Marelli.Domain.Entities.MainData", "MainData")
                        .WithMany("MonthData")
                        .HasForeignKey("MainId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MainData");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Project", b =>
                {
                    b.HasOne("Marelli.Domain.Entities.Group", "Group")
                        .WithMany("Projects")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.UserGroup", b =>
                {
                    b.HasOne("Marelli.Domain.Entities.Group", null)
                        .WithMany("UsersGroup")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Marelli.Domain.Entities.UserProject", b =>
                {
                    b.HasOne("Marelli.Domain.Entities.Project", null)
                        .WithMany("UsersProject")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Week", b =>
                {
                    b.HasOne("Marelli.Domain.Entities.Month", "Month")
                        .WithMany("Weeks")
                        .HasForeignKey("MonthId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Month");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.WeekData", b =>
                {
                    b.HasOne("Marelli.Domain.Entities.MainData", "MainData")
                        .WithMany("WeekData")
                        .HasForeignKey("MainId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MainData");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Group", b =>
                {
                    b.Navigation("Projects");

                    b.Navigation("UsersGroup");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.MainData", b =>
                {
                    b.Navigation("MonthData");

                    b.Navigation("WeekData");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Month", b =>
                {
                    b.Navigation("Days");

                    b.Navigation("Weeks");
                });

            modelBuilder.Entity("Marelli.Domain.Entities.Project", b =>
                {
                    b.Navigation("UsersProject");
                });
#pragma warning restore 612, 618
        }
    }
}
