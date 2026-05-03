import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import Filters from "./Filters";

const { mockMapSearchParamsToFilterValues, mockMapFilterValuesToSearchParams } =
  vi.hoisted(() => ({
    mockMapSearchParamsToFilterValues: vi.fn(),
    mockMapFilterValuesToSearchParams: vi.fn(),
  }));

vi.mock("@/features/search", () => ({
  mapSearchParamsToFilterValues: (...args: unknown[]) =>
    mockMapSearchParamsToFilterValues(...args),
  mapFilterValuesToSearchParams: (...args: unknown[]) =>
    mockMapFilterValuesToSearchParams(...args),
}));

vi.mock("@/constants/uiConstants", () => ({
  UI_CONSTANTS: {
    SELECT: {
      ALL_MAKES: { VALUE: "all", LABEL: "All makes" },
      ALL_MODELS: { VALUE: "all", LABEL: "All models" },
      ANY_LOCATION: { VALUE: "anyLocation", LABEL: "Any location" },
    },
  },
}));

vi.mock("./BasicFilters", () => ({
  default: ({ filters }: { filters: Record<string, string> }) => (
    <div data-testid="basic-filters">
      <span data-testid="make-value">{filters.makeId}</span>
    </div>
  ),
}));

vi.mock("./ModelFilter", () => ({
  default: ({
    makeId,
    filteredModels,
  }: {
    makeId: string;
    filteredModels: string[];
  }) => (
    <div data-testid="model-filter">
      <span data-testid="model-make-id">{makeId}</span>
      <span data-testid="model-count">{filteredModels.length}</span>
    </div>
  ),
}));

vi.mock("./RangeFilters", () => ({
  default: () => <div data-testid="range-filters" />,
}));

describe("Filters", () => {
  const defaultFilterValues = {
    makeId: "all",
    isUsed: "newUsed",
    municipalityId: "anyLocation",
    models: [],
    minYear: "",
    maxYear: "",
    minPrice: "",
    maxPrice: "",
    minMileage: "",
    maxMileage: "",
    minPower: "",
    maxPower: "",
  };

  beforeEach(() => {
    mockMapSearchParamsToFilterValues.mockReturnValue(defaultFilterValues);
    mockMapFilterValuesToSearchParams.mockReturnValue({});
  });

  it("renders BasicFilters and RangeFilters", () => {
    render(<Filters searchParams={{}} onSearchParamChange={vi.fn()} />);

    expect(screen.getByTestId("basic-filters")).toBeInTheDocument();
    expect(screen.getByTestId("range-filters")).toBeInTheDocument();
  });

  it("does not render ModelFilter when makeId is not set in searchParams", () => {
    render(<Filters searchParams={{}} onSearchParamChange={vi.fn()} />);

    expect(screen.queryByTestId("model-filter")).not.toBeInTheDocument();
  });

  it("renders ModelFilter when makeId is present in searchParams", () => {
    mockMapSearchParamsToFilterValues.mockReturnValue({
      ...defaultFilterValues,
      makeId: "make-123",
      models: ["model-1"],
    });

    render(
      <Filters
        searchParams={{ makeId: "make-123" }}
        onSearchParamChange={vi.fn()}
      />,
    );

    expect(screen.getByTestId("model-filter")).toBeInTheDocument();
    expect(screen.getByTestId("model-make-id")).toHaveTextContent("make-123");
  });

  it("passes mapped filter values to BasicFilters", () => {
    mockMapSearchParamsToFilterValues.mockReturnValue({
      ...defaultFilterValues,
      makeId: "make-456",
    });

    render(
      <Filters searchParams={{ makeId: "make-456" }} onSearchParamChange={vi.fn()} />,
    );

    expect(screen.getByTestId("make-value")).toHaveTextContent("make-456");
  });
});
