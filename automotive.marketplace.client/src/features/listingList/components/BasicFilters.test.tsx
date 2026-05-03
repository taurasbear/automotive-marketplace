import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import BasicFilters from "./BasicFilters";

const { mockMakeSelect, mockUsedSelect, mockLocationCombobox } = vi.hoisted(
  () => ({
    mockMakeSelect: vi.fn(),
    mockUsedSelect: vi.fn(),
    mockLocationCombobox: vi.fn(),
  }),
);

vi.mock("@/components/forms/select/MakeSelect", () => ({
  default: (props: Record<string, unknown>) => {
    mockMakeSelect(props);
    return (
      <div data-testid="make-select">
        <button
          onClick={() =>
            (props.onValueChange as (v: string) => void)("make-123")
          }
        >
          Select Make
        </button>
      </div>
    );
  },
}));

vi.mock("@/components/forms/select/UsedSelect", () => ({
  default: (props: Record<string, unknown>) => {
    mockUsedSelect(props);
    return (
      <div data-testid="used-select">
        <button
          onClick={() =>
            (props.onValueChange as (v: string) => void)("used")
          }
        >
          Select Used
        </button>
      </div>
    );
  },
}));

vi.mock("@/components/forms/select/LocationCombobox", () => ({
  default: (props: Record<string, unknown>) => {
    mockLocationCombobox(props);
    return (
      <div data-testid="location-combobox">
        <button
          onClick={() =>
            (props.onValueChange as (v: string) => void)("muni-456")
          }
        >
          Select Location
        </button>
      </div>
    );
  },
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

describe("BasicFilters", () => {
  const defaultFilters = {
    makeId: "all",
    isUsed: "newUsed" as const,
    municipalityId: "anyLocation",
  };

  it("renders MakeSelect, UsedSelect, and LocationCombobox", () => {
    render(
      <BasicFilters filters={defaultFilters} onFilterChange={vi.fn()} />,
    );

    expect(screen.getByTestId("make-select")).toBeInTheDocument();
    expect(screen.getByTestId("used-select")).toBeInTheDocument();
    expect(screen.getByTestId("location-combobox")).toBeInTheDocument();
  });

  it("passes filter values to child components", () => {
    render(
      <BasicFilters filters={defaultFilters} onFilterChange={vi.fn()} />,
    );

    expect(mockMakeSelect).toHaveBeenCalledWith(
      expect.objectContaining({ value: "all" }),
    );
    expect(mockUsedSelect).toHaveBeenCalledWith(
      expect.objectContaining({ value: "newUsed" }),
    );
    expect(mockLocationCombobox).toHaveBeenCalledWith(
      expect.objectContaining({ value: "anyLocation" }),
    );
  });

  it("calls onFilterChange with correct key when make is selected", async () => {
    const mockOnFilterChange = vi.fn();
    render(
      <BasicFilters
        filters={defaultFilters}
        onFilterChange={mockOnFilterChange}
      />,
    );

    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();
    await user.click(screen.getByText("Select Make"));

    expect(mockOnFilterChange).toHaveBeenCalledWith("makeId", "make-123");
  });

  it("calls onFilterChange with correct key when used is selected", async () => {
    const mockOnFilterChange = vi.fn();
    render(
      <BasicFilters
        filters={defaultFilters}
        onFilterChange={mockOnFilterChange}
      />,
    );

    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();
    await user.click(screen.getByText("Select Used"));

    expect(mockOnFilterChange).toHaveBeenCalledWith("isUsed", "used");
  });

  it("calls onFilterChange with correct key when location is selected", async () => {
    const mockOnFilterChange = vi.fn();
    render(
      <BasicFilters
        filters={defaultFilters}
        onFilterChange={mockOnFilterChange}
      />,
    );

    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();
    await user.click(screen.getByText("Select Location"));

    expect(mockOnFilterChange).toHaveBeenCalledWith(
      "municipalityId",
      "muni-456",
    );
  });
});
