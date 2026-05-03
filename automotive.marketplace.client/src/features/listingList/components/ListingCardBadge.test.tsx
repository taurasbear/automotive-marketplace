import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import ListingCardBadge from "./ListingCardBadge";

describe("ListingCardBadge", () => {
  it("renders the title and stat text", () => {
    render(
      <ListingCardBadge
        Icon={<span data-testid="icon">🔧</span>}
        title="Engine"
        stat="2.0 l 150 kW"
      />,
    );

    expect(screen.getByText("Engine")).toBeInTheDocument();
    expect(screen.getByText("2.0 l 150 kW")).toBeInTheDocument();
  });

  it("renders the Icon node", () => {
    render(
      <ListingCardBadge
        Icon={<span data-testid="icon">🔧</span>}
        title="Engine"
        stat="2.0 l 150 kW"
      />,
    );

    expect(screen.getByTestId("icon")).toBeInTheDocument();
  });

  it("renders different prop values correctly", () => {
    render(
      <ListingCardBadge
        Icon={<span data-testid="fuel-icon">⛽</span>}
        title="Fuel Type"
        stat="Diesel"
      />,
    );

    expect(screen.getByText("Fuel Type")).toBeInTheDocument();
    expect(screen.getByText("Diesel")).toBeInTheDocument();
    expect(screen.getByTestId("fuel-icon")).toBeInTheDocument();
  });
});
