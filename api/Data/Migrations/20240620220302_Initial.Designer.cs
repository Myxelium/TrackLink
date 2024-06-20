﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using api.Data;

#nullable disable

namespace api.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240620220302_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("api.Data.Entities.Band", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime");

                    b.Property<string>("Genre")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Image")
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Band", (string)null);
                });

            modelBuilder.Entity("api.Data.Entities.BandMember", b =>
                {
                    b.Property<int>("BandId")
                        .HasColumnType("int");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("MemberId")
                        .HasColumnType("int");

                    b.HasIndex("MemberId");

                    b.ToTable("BandMember", (string)null);
                });

            modelBuilder.Entity("api.Data.Entities.Member", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Fullname")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Image")
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)");

                    b.Property<Guid>("UserIdentifier")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id")
                        .HasName("PK_Members");

                    b.ToTable("Member", (string)null);
                });

            modelBuilder.Entity("api.Data.Entities.MemberRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("MemberId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MemberId");

                    b.HasIndex("RoleId");

                    b.ToTable("MemberRole", (string)null);
                });

            modelBuilder.Entity("api.Data.Entities.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CreatedBy")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CreatedDate")
                        .HasColumnType("datetime");

                    b.Property<int>("CreatedFor")
                        .HasColumnType("int");

                    b.Property<string>("RoleDescription")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("CreatedFor");

                    b.ToTable("Role", (string)null);
                });

            modelBuilder.Entity("api.Data.Entities.Song", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("PreviousVersion")
                        .HasColumnType("int");

                    b.Property<int>("UploadedBy")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("URL");

                    b.Property<int?>("Version")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UploadedBy");

                    b.ToTable("Song", (string)null);
                });

            modelBuilder.Entity("api.Data.Entities.SongIdentifier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BandId")
                        .HasColumnType("int");

                    b.Property<int>("SongId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("SongIdentifier", (string)null);
                });

            modelBuilder.Entity("api.Data.Entities.Vote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("MemberId")
                        .HasColumnType("int");

                    b.Property<int>("SongId")
                        .HasColumnType("int");

                    b.Property<string>("Comment")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id", "MemberId", "SongId");

                    b.HasIndex("MemberId");

                    b.HasIndex("SongId");

                    b.ToTable("Vote", (string)null);
                });

            modelBuilder.Entity("api.Data.Entities.BandMember", b =>
                {
                    b.HasOne("api.Data.Entities.Band", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .IsRequired()
                        .HasConstraintName("FK_BandMember_Band");

                    b.HasOne("api.Data.Entities.Member", "MemberNavigation")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .IsRequired()
                        .HasConstraintName("FK_BandMember_Member");

                    b.Navigation("Member");

                    b.Navigation("MemberNavigation");
                });

            modelBuilder.Entity("api.Data.Entities.MemberRole", b =>
                {
                    b.HasOne("api.Data.Entities.Member", "Member")
                        .WithMany("MemberRoles")
                        .HasForeignKey("MemberId")
                        .IsRequired()
                        .HasConstraintName("FK_MemberRole_Member");

                    b.HasOne("api.Data.Entities.MemberRole", "Role")
                        .WithMany("InverseRole")
                        .HasForeignKey("RoleId")
                        .IsRequired()
                        .HasConstraintName("FK_MemberRole_MemberRole");

                    b.Navigation("Member");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("api.Data.Entities.Role", b =>
                {
                    b.HasOne("api.Data.Entities.Member", "CreatedByNavigation")
                        .WithMany("Roles")
                        .HasForeignKey("CreatedBy")
                        .IsRequired()
                        .HasConstraintName("FK_Role_Member");

                    b.HasOne("api.Data.Entities.Band", "CreatedForNavigation")
                        .WithMany("Roles")
                        .HasForeignKey("CreatedFor")
                        .IsRequired()
                        .HasConstraintName("FK_Role_Band");

                    b.Navigation("CreatedByNavigation");

                    b.Navigation("CreatedForNavigation");
                });

            modelBuilder.Entity("api.Data.Entities.Song", b =>
                {
                    b.HasOne("api.Data.Entities.Member", "UploadedByNavigation")
                        .WithMany("Songs")
                        .HasForeignKey("UploadedBy")
                        .IsRequired()
                        .HasConstraintName("FK_Song_Member");

                    b.Navigation("UploadedByNavigation");
                });

            modelBuilder.Entity("api.Data.Entities.Vote", b =>
                {
                    b.HasOne("api.Data.Entities.Member", "Member")
                        .WithMany("Votes")
                        .HasForeignKey("MemberId")
                        .IsRequired()
                        .HasConstraintName("FK_Vote_Member");

                    b.HasOne("api.Data.Entities.Song", "Song")
                        .WithMany("Votes")
                        .HasForeignKey("SongId")
                        .IsRequired()
                        .HasConstraintName("FK_Vote_Song");

                    b.Navigation("Member");

                    b.Navigation("Song");
                });

            modelBuilder.Entity("api.Data.Entities.Band", b =>
                {
                    b.Navigation("Roles");
                });

            modelBuilder.Entity("api.Data.Entities.Member", b =>
                {
                    b.Navigation("MemberRoles");

                    b.Navigation("Roles");

                    b.Navigation("Songs");

                    b.Navigation("Votes");
                });

            modelBuilder.Entity("api.Data.Entities.MemberRole", b =>
                {
                    b.Navigation("InverseRole");
                });

            modelBuilder.Entity("api.Data.Entities.Song", b =>
                {
                    b.Navigation("Votes");
                });
#pragma warning restore 612, 618
        }
    }
}
