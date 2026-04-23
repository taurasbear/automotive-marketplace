import { getAllDrivetrainsOptions } from "@/api/enum/getAllDrivetrainsOptions";
import { toggleVariants } from "@/components/ui/toggle";
import { cn } from "@/lib/utils";
import * as ToggleGroupPrimitive from "@radix-ui/react-toggle-group";
import { useQuery } from "@tanstack/react-query";
import { type VariantProps } from "class-variance-authority";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";

type DrivetrainToggleGroupProps = React.ComponentProps<
  typeof ToggleGroupPrimitive.Root
> &
  VariantProps<typeof toggleVariants>;

const DrivetrainToggleGroup = ({
  className,
  ...props
}: DrivetrainToggleGroupProps) => {
  const { data: drivetrainsQuery } = useQuery(getAllDrivetrainsOptions);

  const drivetrains = drivetrainsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <ToggleGroup {...props}>
        {drivetrains.map((drivetrain) => (
          <ToggleGroupItem key={drivetrain.id} value={drivetrain.id}>
            {drivetrain.name}
          </ToggleGroupItem>
        ))}
      </ToggleGroup>
    </div>
  );
};

export default DrivetrainToggleGroup;
