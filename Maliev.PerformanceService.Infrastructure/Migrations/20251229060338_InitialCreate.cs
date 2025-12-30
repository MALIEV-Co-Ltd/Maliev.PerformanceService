using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.PerformanceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "performance_improvement_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    initiator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    improvement_areas = table.Column<string>(type: "text", nullable: false),
                    success_criteria = table.Column<string>(type: "text", nullable: false),
                    check_in_notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    outcome = table.Column<int>(type: "integer", nullable: true),
                    extension_count = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_performance_improvement_plans", x => x.id);
                    table.CheckConstraint("CK_PIP_DateRange", "start_date < end_date");
                });

            migrationBuilder.CreateTable(
                name: "performance_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_cycle = table.Column<int>(type: "integer", nullable: false),
                    review_period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    review_period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    self_assessment = table.Column<string>(type: "text", nullable: true),
                    manager_assessment = table.Column<string>(type: "text", nullable: true),
                    overall_rating = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    submitted_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    acknowledged_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_performance_reviews", x => x.id);
                    table.CheckConstraint("CK_PerformanceReview_Period", "review_period_start < review_period_end");
                });

            migrationBuilder.CreateTable(
                name: "goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    performance_review_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    success_criteria = table.Column<string>(type: "text", nullable: true),
                    target_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    current_status = table.Column<int>(type: "integer", nullable: false),
                    progress_updates = table.Column<string>(type: "text", nullable: true),
                    completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_goals", x => x.id);
                    table.CheckConstraint("CK_Goal_TargetDate", "target_completion_date > created_date");
                    table.ForeignKey(
                        name: "fk_goals_performance_reviews_performance_review_id",
                        column: x => x.performance_review_id,
                        principalTable: "performance_reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "review_feedback",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    performance_review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feedback_type = table.Column<int>(type: "integer", nullable: false),
                    feedback = table.Column<string>(type: "text", nullable: false),
                    is_anonymous = table.Column<bool>(type: "boolean", nullable: false),
                    submitted_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_review_feedback", x => x.id);
                    table.ForeignKey(
                        name: "fk_review_feedback_performance_reviews_performance_review_id",
                        column: x => x.performance_review_id,
                        principalTable: "performance_reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_goals_employee",
                table: "goals",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "idx_goals_review",
                table: "goals",
                column: "performance_review_id");

            migrationBuilder.CreateIndex(
                name: "idx_goals_status",
                table: "goals",
                column: "current_status");

            migrationBuilder.CreateIndex(
                name: "idx_pips_employee",
                table: "performance_improvement_plans",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "idx_pips_status",
                table: "performance_improvement_plans",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_perf_reviews_employee",
                table: "performance_reviews",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "idx_perf_reviews_reviewer",
                table: "performance_reviews",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "idx_perf_reviews_status",
                table: "performance_reviews",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_feedback_review",
                table: "review_feedback",
                column: "performance_review_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goals");

            migrationBuilder.DropTable(
                name: "performance_improvement_plans");

            migrationBuilder.DropTable(
                name: "review_feedback");

            migrationBuilder.DropTable(
                name: "performance_reviews");
        }
    }
}
