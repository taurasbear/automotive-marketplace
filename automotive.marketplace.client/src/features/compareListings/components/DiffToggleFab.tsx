import { Button } from "@/components/ui/button";

type DiffToggleFabProps = {
  active: boolean;
  onToggle: () => void;
};

export const DiffToggleFab = ({ active, onToggle }: DiffToggleFabProps) => (
  <Button
    onClick={onToggle}
    variant={active ? "default" : "outline"}
    className="fixed right-6 bottom-6 z-20 shadow-lg"
  >
    {active ? "Show All" : "Diff Only"}
  </Button>
);
