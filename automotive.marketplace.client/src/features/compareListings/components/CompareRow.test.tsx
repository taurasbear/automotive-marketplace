import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { CompareRow } from "./CompareRow";

describe("CompareRow", () => {
  it("renders label and both values", () => {
    render(
      <table>
        <tbody>
          <CompareRow label="Price" valueA="€10,000" valueB="€12,000" diff="equal" />
        </tbody>
      </table>,
    );
    expect(screen.getByText("Price")).toBeInTheDocument();
    expect(screen.getByText("€10,000")).toBeInTheDocument();
    expect(screen.getByText("€12,000")).toBeInTheDocument();
  });

  it("applies green styling to valueA when a-better", () => {
    const { container } = render(
      <table>
        <tbody>
          <CompareRow label="Mileage" valueA="50,000 km" valueB="80,000 km" diff="a-better" />
        </tbody>
      </table>,
    );
    const cells = container.querySelectorAll("td");
    expect(cells[1].className).toContain("text-green-600");
    expect(cells[1].className).toContain("font-semibold");
    expect(cells[2].className).toContain("text-orange-600");
  });

  it("applies green styling to valueB when b-better", () => {
    const { container } = render(
      <table>
        <tbody>
          <CompareRow label="Power" valueA="90 kW" valueB="120 kW" diff="b-better" />
        </tbody>
      </table>,
    );
    const cells = container.querySelectorAll("td");
    expect(cells[2].className).toContain("text-green-600");
    expect(cells[2].className).toContain("font-semibold");
    expect(cells[1].className).toContain("text-orange-600");
  });

  it("applies amber styling when different", () => {
    const { container } = render(
      <table>
        <tbody>
          <CompareRow label="Color" valueA="Red" valueB="Blue" diff="different" />
        </tbody>
      </table>,
    );
    const cells = container.querySelectorAll("td");
    expect(cells[1].className).toContain("bg-amber-50");
    expect(cells[2].className).toContain("bg-amber-50");
  });

  it("adds up/down arrows for a-better diff", () => {
    render(
      <table>
        <tbody>
          <CompareRow label="Year" valueA="2022" valueB="2018" diff="a-better" />
        </tbody>
      </table>,
    );
    expect(screen.getByText(/2022.*↑/)).toBeInTheDocument();
    expect(screen.getByText(/2018.*↓/)).toBeInTheDocument();
  });

  it("adds up/down arrows for b-better diff", () => {
    render(
      <table>
        <tbody>
          <CompareRow label="Fuel" valueA="200" valueB="300" diff="b-better" />
        </tbody>
      </table>,
    );
    expect(screen.getByText(/200.*↓/)).toBeInTheDocument();
    expect(screen.getByText(/300.*↑/)).toBeInTheDocument();
  });

  it("has no background style when equal", () => {
    const { container } = render(
      <table>
        <tbody>
          <CompareRow label="Doors" valueA="4" valueB="4" diff="equal" />
        </tbody>
      </table>,
    );
    const row = container.querySelector("tr")!;
    expect(row.style.backgroundColor).toBe("");
  });

  it("has orange tinted background when not equal", () => {
    const { container } = render(
      <table>
        <tbody>
          <CompareRow label="Doors" valueA="4" valueB="5" diff="a-better" />
        </tbody>
      </table>,
    );
    const row = container.querySelector("tr")!;
    expect(row.style.backgroundColor).toBe("rgba(249, 115, 22, 0.05)");
  });
});
