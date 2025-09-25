import { cn } from "@/lib/utils";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getCarByIdOptions } from "../api/getCarByIdOptions";

type ViewCarDialogContentProps = {
  id: string;
  className?: string;
};

const ViewCarDialogContent = ({ id, className }: ViewCarDialogContentProps) => {
  const { data: carQuery } = useSuspenseQuery(getCarByIdOptions({ id }));

  const car = carQuery.data;

  return (
    <div className={cn(className)}>
      <p>Year: {car?.year}</p>
      <p>Fuel type: {car?.fuel}</p>
      <p>Transmission: {car?.transmission}</p>
      <p>Door count: {car?.doorCount}</p>
      <p>Drivetrain: {car?.drivetrain}</p>
      <p>Body type: {car?.bodyType}</p>
    </div>
  );
};

export default ViewCarDialogContent;
