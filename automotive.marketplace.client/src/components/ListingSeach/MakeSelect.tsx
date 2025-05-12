import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "../ui/select";

const MakeSelect = () => {
  return (
    <div>
      <Select>
        <SelectTrigger>
          <SelectValue placeholder="All" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Makes</SelectLabel>
            <SelectItem value="toyota">Toyota</SelectItem>
            <SelectItem value="honda">Honda</SelectItem>
            <SelectItem value="mazda">Mazda</SelectItem>
            <SelectItem value="ford">Ford</SelectItem>
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default MakeSelect;
