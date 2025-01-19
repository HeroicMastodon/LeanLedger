﻿// <auto-generated />
using System;
using LeanLedgerServer.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LeanLedgerServer.Migrations
{
    [DbContext(typeof(LedgerDbContext))]
    [Migration("20250119042252_AddBudgetCategories")]
    partial class AddBudgetCategories
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("LeanLedgerServer.Accounts.Account", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AccountType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Active")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IncludeInNetWorth")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("OpeningBalance")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("OpeningDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("LeanLedgerServer.Automation.Rule", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Actions")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsStrict")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RuleGroupName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Triggers")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RuleGroupName");

                    b.ToTable("Rules");
                });

            modelBuilder.Entity("LeanLedgerServer.Automation.RuleGroup", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("RuleGroups");
                });

            modelBuilder.Entity("LeanLedgerServer.Budgets.Budget", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Categories")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("ExpectedIncome")
                        .HasColumnType("TEXT");

                    b.Property<int>("Month")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Year")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Budgets");
                });

            modelBuilder.Entity("LeanLedgerServer.TransactionImport.ImportSettings", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AttachedAccountId")
                        .HasColumnType("TEXT");

                    b.Property<char?>("CsvDelimiter")
                        .HasColumnType("TEXT");

                    b.Property<string>("DateFormat")
                        .HasColumnType("TEXT");

                    b.Property<string>("ImportMappings")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AttachedAccountId")
                        .IsUnique();

                    b.ToTable("ImportSettings");
                });

            modelBuilder.Entity("LeanLedgerServer.Transactions.Transaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Amount")
                        .HasColumnType("TEXT");

                    b.Property<string>("Category")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly?>("DateImported")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("DestinationAccountId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("SourceAccountId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UniqueHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DestinationAccountId");

                    b.HasIndex("SourceAccountId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("LeanLedgerServer.Automation.Rule", b =>
                {
                    b.HasOne("LeanLedgerServer.Automation.RuleGroup", "RuleGroup")
                        .WithMany("Rules")
                        .HasForeignKey("RuleGroupName");

                    b.Navigation("RuleGroup");
                });

            modelBuilder.Entity("LeanLedgerServer.TransactionImport.ImportSettings", b =>
                {
                    b.HasOne("LeanLedgerServer.Accounts.Account", "AttachedAccount")
                        .WithOne()
                        .HasForeignKey("LeanLedgerServer.TransactionImport.ImportSettings", "AttachedAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AttachedAccount");
                });

            modelBuilder.Entity("LeanLedgerServer.Transactions.Transaction", b =>
                {
                    b.HasOne("LeanLedgerServer.Accounts.Account", "DestinationAccount")
                        .WithMany("Deposits")
                        .HasForeignKey("DestinationAccountId");

                    b.HasOne("LeanLedgerServer.Accounts.Account", "SourceAccount")
                        .WithMany("Withdrawls")
                        .HasForeignKey("SourceAccountId");

                    b.Navigation("DestinationAccount");

                    b.Navigation("SourceAccount");
                });

            modelBuilder.Entity("LeanLedgerServer.Accounts.Account", b =>
                {
                    b.Navigation("Deposits");

                    b.Navigation("Withdrawls");
                });

            modelBuilder.Entity("LeanLedgerServer.Automation.RuleGroup", b =>
                {
                    b.Navigation("Rules");
                });
#pragma warning restore 612, 618
        }
    }
}
