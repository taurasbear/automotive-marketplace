import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import RangeFilters from "./RangeFilters";

const { mockBasicSelect } = vi.hoisted(() => ({
  mockBasicSelect: vi.fn(),
}));

vi.mock("@/components/forms/select/BasicSelect", () => ({
  default: (props: Record<string, unknown>) => {
    mockBasicSelect(props);
    return (
      <div data-testid={`select-${props.label}`}>
        <button
          onClick={() =>
            (props.onValueChange as (v: string) => void)("test-value")
          }
        >
          {String(props.label)}
        </button>
      </div>
    );
  },
}));

vi.mock("@/utils/rangeUtils", () => ({
  getYearRange: () => ["2020", "2021", "2022"],
  getPriceRange: () => ["1000", "5000", "10000"],
  getMileageRange: () => ["10000", "50000", "100000"],
  getPowerRange: () => ["50", "100", "200"],
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

describe("RangeFilters", () => {
  const defaultFilters = {
    minYear: "",
    maxYear: "",
    minPrice: "",
    maxPrice: "",
    minMileage: "",
    maxMileage: "",
    minPower: "",
    maxPower: "",
  };

  it("renders all four range filter fieldsets", () => {
    render(
      <RangeFilters filters={defaultFilters} onFilterChange={vi.fn()} />,
    );

    expect(screen.getByText("filters.year")).toBeInTheDocument();
    expect(screen.getByText("filters.price")).toBeInTheDocument();
    expect(screen.getByText("filters.mileage")).toBeInTheDocument();
    expect(screen.getByText("filters.powerKw")).toBeInTheDocument();
  });

  it("renders min and max selects for each range", () => {
    render(
      <RangeFilters filters={defaultFilters} onFilterChange={vi.fn()} />,
    );

    const minButtons = screen.getAllByText("filters.min");
    const maxButtons = screen.getAllByText("filters.max");

    expect(minButtons).toHaveLength(4);
    expect(maxButtons).toHaveLength(4);
  });

  it("passes filter values to BasicSelect components", () => {
    const filters = {
      ...defaultFilters,
      minYear: "2020",
      maxPrice: "10000",
    };
    render(<RangeFilters filters={filters} onFilterChange={vi.fn()} />);

    expect(mockBasicSelect).toHaveBeenCalledWith(
      expect.objectContaining({ value: "2020" }),
    );
    expect(mockBasicSelect).toHaveBeenCalledWith(
      expect.objectContaining({ value: "10000" }),
    );
  });

  it("calls onFilterChange when a select value changes", async () => {
    const mockOnFilterChange = vi.fn();
    render(
      <RangeFilters
        filters={defaultFilters}
        onFilterChange={mockOnFilterChange}
      />,
    );

    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();
    const minButtons = screen.getAllByText("filters.min");
    await user.click(minButtons[0]);

    expect(mockOnFilterChange).toHaveBeenCalledWith("minYear", "test-value");
  });
});
