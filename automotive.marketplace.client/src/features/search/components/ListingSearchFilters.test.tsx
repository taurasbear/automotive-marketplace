import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import ListingSearchFilters from "./ListingSearchFilters";
import type { ListingSearchStateValues } from "../types/listingSearchStateValues";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/components/forms/select/MakeSelect", () => ({
  default: ({
    label,
    value,
    onValueChange,
  }: {
    label: string;
    value: string;
    onValueChange: (v: string) => void;
  }) => (
    <div data-testid="make-select">
      <span>{label}</span>
      <span data-testid="make-value">{value}</span>
      <button onClick={() => onValueChange("toyota")}>Select Toyota</button>
    </div>
  ),
}));

vi.mock("@/components/forms/select/ModelSelect", () => ({
  default: ({
    label,
    value,
    onValueChange,
  }: {
    label: string;
    value: string;
    onValueChange: (v: string) => void;
  }) => (
    <div data-testid="model-select">
      <span>{label}</span>
      <span data-testid="model-value">{value}</span>
      <button onClick={() => onValueChange("corolla")}>Select Corolla</button>
    </div>
  ),
}));

vi.mock("@/components/forms/select/BasicSelect", () => ({
  default: ({
    label,
    value,
    onValueChange,
  }: {
    label: string;
    value: string;
    onValueChange: (v: string) => void;
  }) => (
    <div data-testid={`basic-select-${label}`}>
      <span>{label}</span>
      <span data-testid={`value-${label}`}>{value}</span>
      <button onClick={() => onValueChange("2020")}>Select 2020</button>
    </div>
  ),
}));

vi.mock("../../../components/forms/select/LocationCombobox", () => ({
  default: ({
    value,
    onValueChange,
  }: {
    value: string;
    onValueChange: (v: string) => void;
  }) => (
    <div data-testid="location-combobox">
      <span data-testid="location-value">{value}</span>
      <button onClick={() => onValueChange("vilnius")}>Select Vilnius</button>
    </div>
  ),
}));

vi.mock("../../../components/forms/select/UsedSelect", () => ({
  default: ({
    value,
    onValueChange,
  }: {
    value: string;
    onValueChange: (v: string) => void;
  }) => (
    <div data-testid="used-select">
      <span data-testid="used-value">{value}</span>
      <button onClick={() => onValueChange("used")}>Select Used</button>
    </div>
  ),
}));

vi.mock("@/utils/rangeUtils", () => ({
  getYearRange: () => [
    { value: "2020", label: "2020" },
    { value: "2021", label: "2021" },
  ],
  getPriceRange: () => [
    { value: "1000", label: "1000" },
    { value: "5000", label: "5000" },
  ],
}));

const defaultSearchValues: ListingSearchStateValues = {
  makeId: "all",
  models: ["all"],
  municipalityId: "any",
  isUsed: "newUsed",
  minYear: "",
  maxYear: "",
  minPrice: "",
  maxPrice: "",
};

describe("ListingSearchFilters", () => {
  const mockUpdateSearchValue = vi.fn();

  it("renders all filter components", () => {
    render(
      <ListingSearchFilters
        searchValues={defaultSearchValues}
        updateSearchValue={mockUpdateSearchValue}
      />,
    );
    expect(screen.getByTestId("make-select")).toBeInTheDocument();
    expect(screen.getByTestId("model-select")).toBeInTheDocument();
    expect(screen.getByTestId("location-combobox")).toBeInTheDocument();
    expect(screen.getByTestId("used-select")).toBeInTheDocument();
  });

  it("passes search values to child components", () => {
    render(
      <ListingSearchFilters
        searchValues={{ ...defaultSearchValues, makeId: "bmw" }}
        updateSearchValue={mockUpdateSearchValue}
      />,
    );
    expect(screen.getByTestId("make-value")).toHaveTextContent("bmw");
  });

  it("calls updateSearchValue when make is changed", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    render(
      <ListingSearchFilters
        searchValues={defaultSearchValues}
        updateSearchValue={mockUpdateSearchValue}
      />,
    );
    await user.click(screen.getByText("Select Toyota"));
    expect(mockUpdateSearchValue).toHaveBeenCalledWith("makeId", "toyota");
  });

  it("calls updateSearchValue when location is changed", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    render(
      <ListingSearchFilters
        searchValues={defaultSearchValues}
        updateSearchValue={mockUpdateSearchValue}
      />,
    );
    await user.click(screen.getByText("Select Vilnius"));
    expect(mockUpdateSearchValue).toHaveBeenCalledWith(
      "municipalityId",
      "vilnius",
    );
  });

  it("calls updateSearchValue when used/new filter is changed", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    render(
      <ListingSearchFilters
        searchValues={defaultSearchValues}
        updateSearchValue={mockUpdateSearchValue}
      />,
    );
    await user.click(screen.getByText("Select Used"));
    expect(mockUpdateSearchValue).toHaveBeenCalledWith("isUsed", "used");
  });
});
