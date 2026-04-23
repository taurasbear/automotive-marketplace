import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Trash2 } from "lucide-react";
import { useState } from "react";

type SlotEntry = {
  key: number;
  date: string;
  startTime: string;
  endTime: string;
};

type ShareAvailabilityModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (slots: { startTime: string; endTime: string }[]) => void;
};

let nextKey = 0;
const createEmptySlot = (): SlotEntry => ({
  key: nextKey++,
  date: "",
  startTime: "",
  endTime: "",
});

const ShareAvailabilityModal = ({
  open,
  onOpenChange,
  onSubmit,
}: ShareAvailabilityModalProps) => {
  const [slots, setSlots] = useState<SlotEntry[]>([createEmptySlot()]);
  const now = new Date();

  const updateSlot = (
    key: number,
    field: keyof Omit<SlotEntry, "key">,
    value: string,
  ) => {
    setSlots((prev) =>
      prev.map((s) => (s.key === key ? { ...s, [field]: value } : s)),
    );
  };

  const removeSlot = (key: number) => {
    setSlots((prev) => prev.filter((s) => s.key !== key));
  };

  const addSlot = () => {
    setSlots((prev) => [...prev, createEmptySlot()]);
  };

  const validSlots = slots.filter((s) => {
    if (!s.date || !s.startTime || !s.endTime) return false;
    const start = new Date(`${s.date}T${s.startTime}:00Z`);
    const end = new Date(`${s.date}T${s.endTime}:00Z`);
    return start > now && end > start;
  });

  const isValid = validSlots.length > 0 && validSlots.length === slots.length;

  const handleSubmit = () => {
    if (!isValid) return;
    const mapped = slots.map((s) => ({
      startTime: new Date(`${s.date}T${s.startTime}:00Z`).toISOString(),
      endTime: new Date(`${s.date}T${s.endTime}:00Z`).toISOString(),
    }));
    onSubmit(mapped);
    onOpenChange(false);
    setSlots([createEmptySlot()]);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Share Your Availability</DialogTitle>
        </DialogHeader>

        <div className="max-h-80 space-y-3 overflow-y-auto py-2">
          {slots.map((slot, i) => (
            <div key={slot.key} className="bg-muted/50 relative rounded-lg p-3">
              <div className="mb-1 flex items-center justify-between">
                <span className="text-muted-foreground text-xs font-medium">
                  Slot {i + 1}
                </span>
                {slots.length > 1 && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="text-destructive h-6 w-6 p-0"
                    onClick={() => removeSlot(slot.key)}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                )}
              </div>
              <div className="grid grid-cols-[1.4fr_1fr_1fr] gap-2">
                <div className="space-y-1">
                  <Label className="text-xs">Date</Label>
                  <Input
                    type="date"
                    value={slot.date}
                    min={now.toISOString().slice(0, 10)}
                    onChange={(e) =>
                      updateSlot(slot.key, "date", e.target.value)
                    }
                    className="h-8 text-xs"
                  />
                </div>
                <div className="space-y-1">
                  <Label className="text-xs">From</Label>
                  <Input
                    type="time"
                    value={slot.startTime}
                    onChange={(e) =>
                      updateSlot(slot.key, "startTime", e.target.value)
                    }
                    className="h-8 text-xs"
                  />
                </div>
                <div className="space-y-1">
                  <Label className="text-xs">To</Label>
                  <Input
                    type="time"
                    value={slot.endTime}
                    onChange={(e) =>
                      updateSlot(slot.key, "endTime", e.target.value)
                    }
                    className="h-8 text-xs"
                  />
                </div>
              </div>
            </div>
          ))}
        </div>

        <Button
          type="button"
          variant="outline"
          size="sm"
          className="w-full text-xs"
          onClick={addSlot}
        >
          + Add another slot
        </Button>

        <div className="flex justify-end gap-2 pt-1">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={!isValid}>
            Share Availability
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default ShareAvailabilityModal;
