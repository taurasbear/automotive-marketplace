import {
  SelectValue,
  SelectTrigger,
  SelectContent,
  SelectItem,
  Select,
} from "@/components/ui/select";

const UsedSelect = () => {
  return (
    <div>
      <Select>
        <SelectTrigger className="w-full min-w-3xs">
          <SelectValue placeholder="Used & New" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="used">Used</SelectItem>
          <SelectItem value="new">New</SelectItem>
          <SelectItem value="newused">Used & New</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
};

export default UsedSelect;
