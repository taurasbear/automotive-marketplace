import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import { DashboardTile } from "./DashboardTile";

describe("DashboardTile", () => {
  const defaultProps = {
    icon: <span data-testid="tile-icon">📊</span>,
    title: "Offers",
    count: 3,
    subtitle: "3 pending offers",
    detail: "€15,000 — John",
  };

  it("renders title, subtitle, and detail", () => {
    render(<DashboardTile {...defaultProps} />);
    expect(screen.getByText("Offers")).toBeInTheDocument();
    expect(screen.getByText("3 pending offers")).toBeInTheDocument();
    expect(screen.getByText("€15,000 — John")).toBeInTheDocument();
  });

  it("renders icon", () => {
    render(<DashboardTile {...defaultProps} />);
    expect(screen.getByTestId("tile-icon")).toBeInTheDocument();
  });

  it("shows count badge when count > 0", () => {
    render(<DashboardTile {...defaultProps} count={5} />);
    expect(screen.getByText("5")).toBeInTheDocument();
  });

  it("does not show count badge when count is 0", () => {
    render(<DashboardTile {...defaultProps} count={0} />);
    expect(screen.queryByText("0")).not.toBeInTheDocument();
  });

  it("does not render detail when detail is empty", () => {
    render(<DashboardTile {...defaultProps} detail="" />);
    expect(screen.queryByText("€15,000 — John")).not.toBeInTheDocument();
  });

  it("applies highlighted styling when isHighlighted is true", () => {
    render(<DashboardTile {...defaultProps} isHighlighted={true} />);
    const button = screen.getByRole("button");
    expect(button.className).toContain("border-primary/50");
  });

  it("does not apply highlighted styling when isHighlighted is false", () => {
    render(<DashboardTile {...defaultProps} isHighlighted={false} />);
    const button = screen.getByRole("button");
    expect(button.className).not.toContain("border-primary/50");
  });

  it("calls onClick when clicked", async () => {
    const user = userEvent.setup();
    const handleClick = vi.fn();
    render(<DashboardTile {...defaultProps} onClick={handleClick} />);
    await user.click(screen.getByRole("button"));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });
});
