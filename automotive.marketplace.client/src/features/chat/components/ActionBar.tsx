import { Button } from "@/components/ui/button";

const ActionBar = () => (
  <div className="border-border flex items-center gap-2 border-b px-3 py-2">
    <Button variant="outline" size="sm" disabled>
      Make an Offer
    </Button>
    <Button variant="outline" size="sm" disabled>
      More ▾
    </Button>
    <span className="text-muted-foreground ml-auto text-xs">
      More actions coming soon
    </span>
  </div>
);

export default ActionBar;
