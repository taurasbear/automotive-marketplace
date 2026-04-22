import type { DiffResult } from "../types/diff";

type CompareRowProps = {
  label: string;
  valueA: string;
  valueB: string;
  diff: DiffResult;
};

const getCellClass = (side: "a" | "b", diff: DiffResult): string => {
  if (diff === "a-better")
    return side === "a" ? "text-green-600 font-semibold" : "text-orange-600";
  if (diff === "b-better")
    return side === "b" ? "text-green-600 font-semibold" : "text-orange-600";
  if (diff === "different") return "bg-amber-50 dark:bg-amber-950/20";
  return "";
};

const getArrow = (side: "a" | "b", diff: DiffResult): string => {
  if (diff === "a-better") return side === "a" ? " ↑" : " ↓";
  if (diff === "b-better") return side === "b" ? " ↑" : " ↓";
  return "";
};

const getRowStyle = (diff: DiffResult): React.CSSProperties =>
  diff !== "equal" ? { backgroundColor: "rgba(249,115,22,0.05)" } : {};

export const CompareRow = ({ label, valueA, valueB, diff }: CompareRowProps) => (
  <tr
    style={getRowStyle(diff)}
    className="divide-x divide-border border-b border-border"
  >
    <td className="px-4 py-3 text-sm font-medium text-muted-foreground">{label}</td>
    <td className={`px-4 py-3 text-sm text-center ${getCellClass("a", diff)}`}>
      {valueA}
      {getArrow("a", diff)}
    </td>
    <td className={`px-4 py-3 text-sm text-center ${getCellClass("b", diff)}`}>
      {valueB}
      {getArrow("b", diff)}
    </td>
  </tr>
);
