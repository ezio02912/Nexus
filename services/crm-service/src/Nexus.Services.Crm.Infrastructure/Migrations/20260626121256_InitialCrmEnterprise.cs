using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Services.Crm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCrmEnterprise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crm_activities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    related_entity_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    activity_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletion_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_activities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    customer_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    tax_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    website = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    industry = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    address_line1 = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    address_line2 = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    city = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    state = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    country = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    rating = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletion_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    source_service = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "leads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    company_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    lead_score = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    converted_customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    converted_opportunity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    converted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lost_reason = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    city = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    country = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletion_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_service = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "pipeline_stages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    probability_default = table.Column<int>(type: "integer", nullable: false),
                    is_won = table.Column<bool>(type: "boolean", nullable: false),
                    is_lost = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletion_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pipeline_stages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    mobile = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    position = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    department = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    is_decision_maker = table.Column<bool>(type: "boolean", nullable: false),
                    linked_in_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletion_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacts", x => x.id);
                    table.ForeignKey(
                        name: "FK_contacts_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "opportunities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    stage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    probability = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    expected_close_date = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_close_date = table.Column<DateOnly>(type: "date", nullable: true),
                    close_reason = table.Column<string>(type: "text", nullable: true),
                    lost_reason = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    next_step = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    next_step_date = table.Column<DateOnly>(type: "date", nullable: true),
                    source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    competitor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletion_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opportunities", x => x.id);
                    table.ForeignKey(
                        name: "FK_opportunities_contacts_contact_id",
                        column: x => x.contact_id,
                        principalTable: "contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_opportunities_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_opportunities_leads_lead_id",
                        column: x => x.lead_id,
                        principalTable: "leads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quotations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    opportunity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quotation_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    rejected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    terms = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletion_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotations", x => x.id);
                    table.ForeignKey(
                        name: "FK_quotations_contacts_contact_id",
                        column: x => x.contact_id,
                        principalTable: "contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quotations_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quotations_opportunities_opportunity_id",
                        column: x => x.opportunity_id,
                        principalTable: "opportunities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    opportunity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contract_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    contract_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    renewal_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    signed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    signed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    termination_reason = table.Column<string>(type: "text", nullable: true),
                    payment_terms = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    terms = table.Column<string>(type: "text", nullable: true),
                    file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletion_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.id);
                    table.ForeignKey(
                        name: "FK_contracts_contacts_contact_id",
                        column: x => x.contact_id,
                        principalTable: "contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contracts_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contracts_opportunities_opportunity_id",
                        column: x => x.opportunity_id,
                        principalTable: "opportunities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contracts_quotations_quotation_id",
                        column: x => x.quotation_id,
                        principalTable: "quotations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quotation_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_no = table.Column<int>(type: "integer", nullable: false),
                    product_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    product_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotation_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_quotation_lines_quotations_quotation_id",
                        column: x => x.quotation_id,
                        principalTable: "quotations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contract_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_no = table.Column<int>(type: "integer", nullable: false),
                    product_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    product_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_percent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_contract_lines_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contacts_customer_id",
                table: "contacts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_tenant_id",
                table: "contacts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_tenant_id_customer_id",
                table: "contacts",
                columns: new[] { "tenant_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "IX_contacts_tenant_id_owner_id",
                table: "contacts",
                columns: new[] { "tenant_id", "owner_id" });

            migrationBuilder.CreateIndex(
                name: "IX_contract_lines_contract_id_line_no",
                table: "contract_lines",
                columns: new[] { "contract_id", "line_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contract_lines_tenant_id_contract_id",
                table: "contract_lines",
                columns: new[] { "tenant_id", "contract_id" });

            migrationBuilder.CreateIndex(
                name: "IX_contracts_contact_id",
                table: "contracts",
                column: "contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_customer_id",
                table: "contracts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_opportunity_id",
                table: "contracts",
                column: "opportunity_id");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_quotation_id",
                table: "contracts",
                column: "quotation_id");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_tenant_id_contract_no",
                table: "contracts",
                columns: new[] { "tenant_id", "contract_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contracts_tenant_id_customer_id",
                table: "contracts",
                columns: new[] { "tenant_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "IX_contracts_tenant_id_end_date",
                table: "contracts",
                columns: new[] { "tenant_id", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_contracts_tenant_id_status",
                table: "contracts",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_tenant_id",
                table: "crm_activities",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_tenant_id_activity_date",
                table: "crm_activities",
                columns: new[] { "tenant_id", "activity_date" });

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_tenant_id_related_entity_type_related_entity~",
                table: "crm_activities",
                columns: new[] { "tenant_id", "related_entity_type", "related_entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_tenant_id_status",
                table: "crm_activities",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_customers_tenant_id_code",
                table: "customers",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_tenant_id_name",
                table: "customers",
                columns: new[] { "tenant_id", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_customers_tenant_id_owner_id",
                table: "customers",
                columns: new[] { "tenant_id", "owner_id" });

            migrationBuilder.CreateIndex(
                name: "IX_leads_tenant_id",
                table: "leads",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_leads_tenant_id_owner_id",
                table: "leads",
                columns: new[] { "tenant_id", "owner_id" });

            migrationBuilder.CreateIndex(
                name: "IX_leads_tenant_id_status",
                table: "leads",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_contact_id",
                table: "opportunities",
                column: "contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_customer_id",
                table: "opportunities",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_lead_id",
                table: "opportunities",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_tenant_id",
                table: "opportunities",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_tenant_id_customer_id",
                table: "opportunities",
                columns: new[] { "tenant_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_tenant_id_owner_id",
                table: "opportunities",
                columns: new[] { "tenant_id", "owner_id" });

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_tenant_id_stage",
                table: "opportunities",
                columns: new[] { "tenant_id", "stage" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_unpublished",
                table: "outbox_messages",
                column: "occurred_at",
                filter: "published_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_stages_tenant_id_entity_type_code",
                table: "pipeline_stages",
                columns: new[] { "tenant_id", "entity_type", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_stages_tenant_id_entity_type_sort_order",
                table: "pipeline_stages",
                columns: new[] { "tenant_id", "entity_type", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_quotation_lines_quotation_id_line_no",
                table: "quotation_lines",
                columns: new[] { "quotation_id", "line_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quotation_lines_tenant_id_quotation_id",
                table: "quotation_lines",
                columns: new[] { "tenant_id", "quotation_id" });

            migrationBuilder.CreateIndex(
                name: "IX_quotations_contact_id",
                table: "quotations",
                column: "contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_quotations_customer_id",
                table: "quotations",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_quotations_opportunity_id",
                table: "quotations",
                column: "opportunity_id");

            migrationBuilder.CreateIndex(
                name: "IX_quotations_tenant_id_customer_id",
                table: "quotations",
                columns: new[] { "tenant_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "IX_quotations_tenant_id_quotation_no",
                table: "quotations",
                columns: new[] { "tenant_id", "quotation_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quotations_tenant_id_status",
                table: "quotations",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_lines");

            migrationBuilder.DropTable(
                name: "crm_activities");

            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "pipeline_stages");

            migrationBuilder.DropTable(
                name: "quotation_lines");

            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "quotations");

            migrationBuilder.DropTable(
                name: "opportunities");

            migrationBuilder.DropTable(
                name: "contacts");

            migrationBuilder.DropTable(
                name: "leads");

            migrationBuilder.DropTable(
                name: "customers");
        }
    }
}
