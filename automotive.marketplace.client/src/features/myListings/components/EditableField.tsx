import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Toggle } from "@/components/ui/toggle";
import { formatNumber } from "@/lib/i18n/formatNumber";
import { Check, Pencil, X } from "lucide-react";
import { useState } from "react";

type EditableFieldProps = {
  label: string;
  value: string | number | boolean;
  displayValue?: string;
  type: "text" | "number" | "textarea" | "toggle";
  toggleLabels?: { on: string; off: string };
  onConfirm: (newValue: string | number | boolean) => void;
};

const EditableField = ({
  label,
  value,
  displayValue,
  type,
  toggleLabels,
  onConfirm,
}: EditableFieldProps) => {
  const [isEditing, setIsEditing] = useState(false);
  const [editValue, setEditValue] = useState(value);

  const handleEdit = () => {
    setEditValue(value);
    setIsEditing(true);
  };

  const handleCancel = () => {
    setEditValue(value);
    setIsEditing(false);
  };

  const handleConfirm = () => {
    onConfirm(editValue);
    setIsEditing(false);
  };

  const formatDisplayValue = () => {
    if (displayValue) return displayValue;
    if (type === "toggle" && toggleLabels) {
      return value ? toggleLabels.on : toggleLabels.off;
    }
    if (type === "number" && typeof value === "number") {
      return formatNumber(value);
    }
    return String(value);
  };

  if (isEditing) {
    return (
      <div className="space-y-2">
        <label className="text-muted-foreground text-sm">{label}</label>
        <div className="flex items-center gap-2">
          {type === "text" && (
            <Input
              value={String(editValue)}
              onChange={(e) => setEditValue(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleConfirm();
                if (e.key === "Escape") handleCancel();
              }}
              autoFocus
              className="flex-1"
            />
          )}
          {type === "number" && (
            <Input
              type="number"
              value={String(editValue)}
              onChange={(e) => setEditValue(Number(e.target.value) || 0)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleConfirm();
                if (e.key === "Escape") handleCancel();
              }}
              autoFocus
              className="flex-1"
            />
          )}
          {type === "textarea" && (
            <Textarea
              value={String(editValue)}
              onChange={(e) => setEditValue(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter" && e.metaKey) handleConfirm();
                if (e.key === "Escape") handleCancel();
              }}
              autoFocus
              className="flex-1"
              rows={3}
            />
          )}
          {type === "toggle" && toggleLabels && (
            <div className="flex flex-1 items-center gap-2">
              <Toggle
                pressed={Boolean(editValue)}
                onPressedChange={setEditValue}
                variant="outline"
              >
                {editValue ? toggleLabels.on : toggleLabels.off}
              </Toggle>
            </div>
          )}
          <Button size="sm" onClick={handleConfirm} variant="outline">
            <Check className="h-4 w-4" />
          </Button>
          <Button size="sm" onClick={handleCancel} variant="outline">
            <X className="h-4 w-4" />
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex items-start justify-between py-2">
      <div>
        <div className="text-muted-foreground text-sm">{label}</div>
        <div className="font-medium">{formatDisplayValue()}</div>
      </div>
      <Button size="sm" variant="ghost" onClick={handleEdit}>
        <Pencil className="h-4 w-4" />
      </Button>
    </div>
  );
};

export default EditableField;
